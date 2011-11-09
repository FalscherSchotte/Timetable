using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TimetablePlanner
{
    public class TimetableGenerator
    {
        #region Variables

        private TimetableData ttData;
        private static Random random;
        private static int numberOfDays = 5;

        #endregion

        #region Properties

        /// <summary>
        /// Current generation in evolution
        /// </summary>
        public int CurrentGeneration { get; private set; }

        /// <summary>
        /// Population containing all current available individuals
        /// </summary>
        public Individual[] Population { get; private set; }

        /// <summary>
        /// Evolution history
        /// </summary>
        public List<HistoryEntry> EvolutionHistory;

        #endregion

        #region Events

        /// <summary>
        /// Individual created
        /// </summary>
        public event Action<int> IndividualCreated;

        /// <summary>
        /// Generation finished
        /// </summary>
        public event Action<HistoryEntry> GenerationTick;

        #endregion

        #region Structs

        /// <summary>
        /// Evolution history element
        /// </summary>
        public struct HistoryEntry
        {
            public int Index;
            public int AverageFitness;
            public int BestFitness;
            public DateTime CreationDate;

            public HistoryEntry(int generation, int average, int best, DateTime creation)
            {
                Index = generation;
                AverageFitness = average;
                BestFitness = best;
                CreationDate = creation;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create new timetable generator
        /// </summary>
        /// <param name="populationSize">size of the population to generate</param>
        /// <param name="tableData">timetable configuration</param>
        public TimetableGenerator(TimetableData tableData)
        {
            if (random == null)
                random = new Random(DateTime.Now.Millisecond);
            this.ttData = tableData;
        }

        #endregion

        #region Population creation

        /// <summary>
        /// Create Population with specified size and fill it random
        /// </summary>
        /// <param name="size">Size of the population</param>
        public void CreatePopulation(int size, int numberOfThreads)
        {
            List<Individual> populationList = new List<Individual>(size);
            List<Thread> creationThreads = new List<Thread>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(CreateIndividualsUntilPopulationFilled));
                t.Name = "Individual creation thread " + i.ToString("00");
                creationThreads.Add(t);
            }
            foreach (Thread t in creationThreads)
            {
                t.Start(populationList);
            }
            foreach (Thread t in creationThreads)
            {
                t.Join();
            }

            Population = populationList.ToArray();
            CalculateFitness(Population);
            SortIndividuals(Population);
        }

        /// <summary>
        /// Creates a new individuals until the population reached its full size
        /// </summary>
        private void CreateIndividualsUntilPopulationFilled(Object parameter)
        {
            while (true)
            {
                int index = 0;
                lock (parameter)
                {
                    if ((parameter as List<Individual>).Count == (parameter as List<Individual>).Capacity)
                        return;
                    (parameter as List<Individual>).Add(new Individual(numberOfDays, ttData.Blocks.Length,
                        ttData.Courses.Length, ttData.Lecturers.Length,
                        ttData.Rooms.Length, ttData.Groups.Length));
                    index = (parameter as List<Individual>).Count - 1;
                }

                bool success = false;
                do
                {
                    success = RandomFillIndividual((parameter as List<Individual>)[index]);
                } while (!success);

                if (IndividualCreated != null)
                    IndividualCreated(index);
            }
        }

        /// <summary>
        /// Random fill the given individual
        /// </summary>
        /// <param name="individual">individual to be filled</param>
        /// <returns>success</returns>
        private bool RandomFillIndividual(Individual individual)
        {
            for (int courseIndex = 0; courseIndex < individual.Courses.GetLength(0); courseIndex++)
            {
                List<PlacementContainer> possibilities = GetPossibilitiesForCourse(courseIndex, individual);
                if (possibilities.Count <= 0)
                {
                    //When no possibilities left, restart
                    individual.Clear();
                    return false;
                }

                int chosenPossibility = random.Next(0, possibilities.Count);
                for (int blockOffset = 0; blockOffset < ttData.Courses[courseIndex].NumberOfBlocks; blockOffset++)
                {
                    individual.SetChromosome(courseIndex,
                        possibilities[chosenPossibility].day,
                        possibilities[chosenPossibility].block + blockOffset,
                        possibilities[chosenPossibility].room,
                        ttData.Courses[courseIndex].Group.Index,
                        GetLecturerIndices(courseIndex));
                }
            }
            return true;
        }

        /// <summary>
        /// Get all indices of the lecturers of the given course
        /// </summary>
        /// <param name="courseIndex">Index of the course to be inspected</param>
        /// <returns>List of the lecturer indices</returns>
        private List<int> GetLecturerIndices(int courseIndex)
        {
            List<int> lecturers = new List<int>();
            for (int lecturerIndex = 0; lecturerIndex < ttData.Courses[courseIndex].Lecturers.Length; lecturerIndex++)
            {
                lecturers.Add(ttData.Courses[courseIndex].Lecturers[lecturerIndex].Index);
            }
            return lecturers;
        }

        /// <summary>
        /// Container for a possibility for course placement
        /// </summary>
        private struct PlacementContainer
        {
            public int day;
            public int block;
            public int room;
        }

        /// <summary>
        /// Get a list of all possiblities for course placement
        /// </summary>
        /// <param name="course">course to inspect</param>
        /// <param name="individual">individual to be inspected</param>
        /// <returns>List of the possibilities</returns>
        private List<PlacementContainer> GetPossibilitiesForCourse(int course, Individual individual)
        {
            List<PlacementContainer> possibilities = new List<PlacementContainer>();
            int neededNumberOfBlocks = ttData.Courses[course].NumberOfBlocks;
            for (int day = 0; day < individual.Courses.GetLength(1); day++)
            {
                for (int block = 0; block < individual.Courses.GetLength(2); block++)
                {
                    if (block + neededNumberOfBlocks - 1 < individual.Courses.GetLength(2))
                    {
                        for (int room = 0; room < individual.Rooms.GetLength(0); room++)
                        {
                            if (IsValidForCourse(course, day, block, room, individual))
                            {
                                PlacementContainer c = new PlacementContainer();
                                c.day = day;
                                c.block = block;
                                c.room = room;
                                possibilities.Add(c);
                            }
                        }
                    }
                }
            }
            return possibilities;
        }

        /// <summary>
        /// Checks if the placement for the course is valid
        /// </summary>
        /// <param name="course"></param>
        /// <param name="day"></param>
        /// <param name="block"></param>
        /// <param name="room"></param>
        /// <param name="individual"></param>
        /// <returns>Placement is valid</returns>
        private bool IsValidForCourse(int course, int day, int block, int room, Individual individual)
        {
            for (int blockOffset = 0; blockOffset < ttData.Courses[course].NumberOfBlocks; blockOffset++)
            {
                //block available at that day?
                foreach (DayOfWeek exceptionDay in ttData.Blocks[block + blockOffset].Exceptions)
                {
                    if ((int)exceptionDay == day + 1)
                        return false;
                }

                //Course already set?
                if (individual.Courses[course, day, block + blockOffset] != -1)
                    return false;

                //Room already occupied?
                if (individual.Rooms[room, day, block + blockOffset] != -1)
                    return false;

                //Lab?
                if (ttData.Courses[course].NeedsLab != ttData.Rooms[room].IsLab)
                    return false;

                //Lecturers available?
                for (int neededLecturer = 0; neededLecturer < ttData.Courses[course].Lecturers.Length; neededLecturer++)
                {
                    if (!ttData.Courses[course].Lecturers[neededLecturer].IsDummy)
                    {
                        if (individual.Lecturers[ttData.Courses[course].Lecturers[neededLecturer].Index, day, block + blockOffset] != -1)
                            return false;
                    }
                }

                //Group available?
                if (individual.Groups[ttData.Courses[course].Group.Index, day, block + blockOffset] != -1)
                    return false;
            }

            //Researchdays restriction for lecturers
            foreach (Lecturer l in ttData.Courses[course].Lecturers)
            {
                int tmp = individual.Lecturers[l.Index, day, block];
                individual.Lecturers[l.Index, day, block] = course;
                int freeDays = GetFreeDaysForLecturer(individual, l.Index, ttData);
                individual.Lecturers[l.Index, day, block] = tmp;
                if (freeDays < l.NeededNumberOfResearchDays)
                    return false;
            }

            return true;
        }

        private static int GetFreeDaysForLecturer(Individual individual, int lIndex, TimetableData ttData)
        {
            int freeDayCount = 0;
            for (int d = 0; d < individual.Lecturers.GetLength(1); d++)
            {
                if (ttData.Lecturers[lIndex].AvailableResearchDays.Contains(d))
                {
                    bool isFree = true;
                    for (int b = 0; b < individual.Lecturers.GetLength(2); b++)
                    {
                        if (individual.Lecturers[lIndex, d, b] != -1)
                        {
                            isFree = false;
                        }
                    }
                    if (isFree)
                        freeDayCount++;
                }
            }
            return freeDayCount;
        }

        #endregion

        #region Evolution

        /// <summary>
        /// Perform evolution
        /// </summary>
        public void PerformEvolution(int numberOfGenerations)
        {
            EvolutionHistory = new List<HistoryEntry>();
            EvolutionHistory.Add(new HistoryEntry(0, CalcAverageFitness(Population), Population[0].Fitness, DateTime.Now));

            for (CurrentGeneration = 0; CurrentGeneration < numberOfGenerations; CurrentGeneration++)
            {
                List<Individual> individuals = new List<Individual>();
                foreach (Individual individual in Population)
                {
                    individuals.Add(PerformMutation(individual));
                }
                CalculateFitness(individuals);
                individuals.AddRange(Population);
                SortIndividuals(individuals);

                //Population = RouletteSelection(Population.Length, individuals).ToArray(); 
                //Rouletteselection is not working as good as best of n
                Population = individuals.GetRange(0, Population.Length).ToArray();

                EvolutionHistory.Add(new HistoryEntry(CurrentGeneration, CalcAverageFitness(Population), Population[0].Fitness, DateTime.Now));
                if (GenerationTick != null)
                    GenerationTick(EvolutionHistory[EvolutionHistory.Count - 1]);

                if (MeanAverageFitnessIncreaseOfTheLastNGenerations(10) < 2)
                    break;
            }
        }

        /// <summary>
        /// Calculates the mean average fitness increase of the last n generations in evolution history
        /// </summary>
        /// <param name="n">Number of generations</param>
        /// <returns>Mean increase</returns>
        private int MeanAverageFitnessIncreaseOfTheLastNGenerations(int n)
        {
            if (EvolutionHistory.Count < 2)
                return EvolutionHistory[EvolutionHistory.Count - 1].AverageFitness;

            n = Math.Min(n, EvolutionHistory.Count - 1);
            int sum = 0;
            for (int generation = 0; generation < n; generation++)
            {
                sum += EvolutionHistory[EvolutionHistory.Count - 1 - generation].AverageFitness - EvolutionHistory[EvolutionHistory.Count - 2 - generation].AverageFitness;
            }
            return sum / n;
        }

        /// <summary>
        /// Calculates the average fitness
        /// </summary>
        /// <param name="source">Collection of individuals to inspect</param>
        /// <returns>Average fitness</returns>
        private int CalcAverageFitness(IEnumerable<Individual> source)
        {
            return source.Sum<Individual>(p => p.Fitness) / source.Count<Individual>();
        }

        /// <summary>
        /// Creates a mutation of the given individual
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        private Individual PerformMutation(Individual individual)
        {
            Individual mutation = Individual.Clone(individual);
            int chosenCourse = random.Next(0, mutation.Courses.GetLength(0));

            List<PlacementContainer> possibilities = GetPossibilitiesForCourse(chosenCourse, mutation);
            if (possibilities.Count <= 0)
                return mutation;
            int chosenPossibility = random.Next(0, possibilities.Count);

            mutation.ClearChromosome(chosenCourse, ttData);

            List<int> lecturers = new List<int>();
            foreach (Lecturer lecturer in ttData.Courses[chosenCourse].Lecturers)
            {
                lecturers.Add(lecturer.Index);
            }

            for (int blockOffset = 0; blockOffset < ttData.Courses[chosenCourse].NumberOfBlocks; blockOffset++)
            {
                mutation.SetChromosome(chosenCourse,
                    possibilities[chosenPossibility].day,
                    possibilities[chosenPossibility].block + blockOffset,
                    possibilities[chosenPossibility].room,
                    ttData.Courses[chosenCourse].Group.Index,
                    lecturers);
            }

            return mutation;
        }

        private List<Individual> RouletteSelection(int numberOfIndividualsToChoose, List<Individual> source)
        {
            List<Individual> individuals = new List<Individual>();
            int totalFitness = 0;
            for (int i = 0; i < source.Count; i++)
            {
                totalFitness += source[i].Fitness * i; //Scaling the values with their ranking
            }

            for (int numberOfRolls = 0; numberOfRolls < numberOfIndividualsToChoose; numberOfRolls++)
            {
                int ivoryBall = random.Next(1, totalFitness);
                int currentFitness = 0;
                for (int individualIndex = source.Count - 1; individualIndex >= 0; individualIndex--)
                {
                    if ((currentFitness += source[individualIndex].Fitness * (source.Count - individualIndex)) >= ivoryBall)
                    {
                        individuals.Add(source[individualIndex]);
                        break;
                    }
                }
            }
            SortIndividuals(individuals);
            return individuals;
        }

        #endregion

        #region Fitness calculation

        private void CalculateFitness(List<Individual> individuals)
        {
            foreach (Individual individual in individuals)
            {
                CalculateFitness(individual);
            }
        }

        private void CalculateFitness(Individual[] individuals)
        {
            foreach (Individual individual in individuals)
            {
                CalculateFitness(individual);
            }
        }

        private void SortIndividuals(List<Individual> individuals)
        {
            individuals.Sort(SortByFitness);
        }

        private void SortIndividuals(Individual[] individuals)
        {
            Array.Sort(individuals, SortByFitness);
        }

        private static int SortByFitness(Individual x, Individual y)
        {
            if (x.Fitness > y.Fitness)
                return -1;
            if (x.Fitness < y.Fitness)
                return +1;
            return 0;
        }

        private void CalculateFitness(Individual individual)
        {
            int fitness = 0;
            for (int day = 0; day < numberOfDays; day++)
            {
                Dictionary<int, int> groupFirstBlock = new Dictionary<int, int>();
                Dictionary<int, int> groupLastBlock = new Dictionary<int, int>();
                Dictionary<int, int> groupBlockCount = new Dictionary<int, int>();
                Dictionary<string, int> courseIdBlackList = new Dictionary<string, int>();
                Dictionary<int, List<int>> courseBlockStartHours = new Dictionary<int, List<int>>();

                for (int block = 0; block < ttData.Blocks.Length; block++)
                {
                    for (int course = 0; course < individual.Courses.GetLength(0); course++)
                    {
                        if (individual.Courses[course, day, block] != -1)
                        {
                            //Courses should not appear more than once a day
                            if (!courseIdBlackList.ContainsKey(ttData.Courses[course].Id))
                                courseIdBlackList[ttData.Courses[course].Id] = 0;
                            courseIdBlackList[ttData.Courses[course].Id]++;
                            if (courseIdBlackList[ttData.Courses[course].Id] > ttData.Courses[course].NumberOfBlocks)
                                fitness -= 100;

                            //Courses should start before 13:00
                            //Opposite for dummy courses
                            fitness += (ttData.Blocks[block].Start.Hour - 13) * 15 * (ttData.Courses[course].IsDummy ? 1 : -1);

                            //Roompreference
                            if (ttData.Courses[course].RoomPreference != null)
                            {
                                if (individual.Courses[course, day, block] == ttData.Courses[course].RoomPreference.Index)
                                    fitness += 100;
                            }

                            //Measurements that are not applied on dummys
                            if (!ttData.Courses[course].IsDummy)
                            {
                                //Store the first and last block of the group
                                int group = ttData.Courses[course].Group.Index;
                                if (!groupFirstBlock.ContainsKey(group))
                                    groupFirstBlock.Add(group, block);
                                if (!groupLastBlock.ContainsKey(group))
                                    groupLastBlock.Add(group, block);
                                if (!groupBlockCount.ContainsKey(group))
                                    groupBlockCount.Add(group, 1);
                                groupLastBlock[group] = block;
                                groupBlockCount[group]++;

                                //A course should not by-pass the lunch break at 13:00
                                //--> Store the start hours
                                if (!courseBlockStartHours.ContainsKey(course))
                                    courseBlockStartHours.Add(course, new List<int>());
                                courseBlockStartHours[course].Add(ttData.Blocks[block].Start.Hour);
                            }
                        }
                    }
                }

                //Groups should not have gaps in their plan
                for (int group = 0; group < ttData.Groups.Length; group++)
                {
                    if (groupFirstBlock.ContainsKey(group))
                        fitness -= (groupLastBlock[group] - groupFirstBlock[group] - groupBlockCount[group]) * 30;
                }

                //A course should not by-pass the lunch break at 13:00
                foreach (var key in courseBlockStartHours.Keys)
                {
                    int start = courseBlockStartHours[key][0];
                    int end = courseBlockStartHours[key][courseBlockStartHours[key].Count - 1];
                    if (start - 13 < 0 && !(end - 13 < 0))
                        fitness -= 50;
                }
            }

            for (int g = 0; g < individual.Groups.GetLength(0); g++)
            {
                for (int d = 0; d < individual.Groups.GetLength(1); d++)
                {
                    int count = 0;
                    for (int b = 0; b < individual.Groups.GetLength(2); b++)
                    {
                        if (individual.Groups[g, d, b] != -1)
                        {
                            if (!ttData.Courses[individual.Groups[g, d, b]].IsDummy)
                                count++;
                        }
                    }
                    //Increase fitness when a group gets a free day
                    if (count == 0)
                        fitness += 100;
                }
            }

            individual.Fitness = fitness;
        }

        #endregion
    }
}
