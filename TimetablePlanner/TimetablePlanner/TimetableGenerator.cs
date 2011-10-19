using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public int CurrentGeneration { get; private set; }
        public Individual[] Population { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create new timetable generator
        /// </summary>
        /// <param name="populationSize">size of the population to generate</param>
        /// <param name="tableData">timetable configuration</param>
        public TimetableGenerator(int populationSize, TimetableData tableData)
        {
            if (random == null)
                random = new Random(DateTime.Now.Millisecond);
            this.ttData = tableData;
            CreatePopulation(populationSize);
            SortIndividuals(Population);
        }

        #endregion

        #region Population creation

        /// <summary>
        /// Create Population with specified size and fill it random
        /// </summary>
        /// <param name="size">Size of the population</param>
        private void CreatePopulation(int size)
        {
            Population = new Individual[size];
            for (int i = 0; i < size; i++)
            {
                Population[i] = new Individual(numberOfDays, ttData.Blocks.Length,
                    ttData.Courses.Length, ttData.Lecturers.Length,
                    ttData.Rooms.Length, ttData.Groups.Length);

                bool success = false;
                do
                {
                    success = RandomFillIndividual(Population[i]);
                } while (!success);
            }
            CalculateFitness(Population);
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
                List<PossibilitiyContainer> possibilities = GetPossibilitiesForCourse(courseIndex, individual);
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
        private struct PossibilitiyContainer
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
        private List<PossibilitiyContainer> GetPossibilitiesForCourse(int course, Individual individual)
        {
            List<PossibilitiyContainer> possibilities = new List<PossibilitiyContainer>();
            int neededNumberOfBlocks = ttData.Courses[course].NumberOfBlocks;
            for (int day = 0; day < individual.Courses.GetLength(1); day++)
            {
                for (int block = 0; block < individual.Courses.GetLength(2); block++)
                {
                    if (block + neededNumberOfBlocks < individual.Courses.GetLength(2))
                    {
                        for (int room = 0; room < individual.Rooms.GetLength(0); room++)
                        {
                            if (IsValidForCourse(course, day, block, room, individual))
                            {
                                PossibilitiyContainer c = new PossibilitiyContainer();
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
                    if (individual.Lecturers[ttData.Courses[course].Lecturers[neededLecturer].Index, day, block + blockOffset] != -1)
                        return false;
                }

                //group available?
                if (individual.Groups[ttData.Courses[course].Group.Index, day, block + blockOffset] != -1)
                    return false;
            }
            return true;
        }

        #endregion

        #region Evolution

        /// <summary>
        /// Perform evolution
        /// </summary>
        public void PerformEvolution(int numberOfGenerations)
        {
            List<int> fitnessHistory = new List<int>();
            int top5FitnessChangesCtr = 0;
            int top5Fitness = 0;
            long start = DateTime.Now.Ticks;

            fitnessHistory.Add(CalcAverageFitness(Population));
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
                Population = individuals.GetRange(0, Population.Length).ToArray();

                fitnessHistory.Add(CalcAverageFitness(Population));

                if (TestEarlyAbortOfEvolution(ref top5FitnessChangesCtr, ref top5Fitness, CurrentGeneration))
                    break;
            }

            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Mean time per generation: " + (end - start) / 10000 / CurrentGeneration + "ms");
            PrintFitnessIncrease(fitnessHistory);
        }

        private int CalcAverageFitness(Individual[] population)
        {
            int average = 0;
            foreach (var individual in population)
            {
                average += individual.Fitness;
            }
            return average / population.Length;
        }

        private void PrintFitnessIncrease(List<int> fitnessHistory)
        {
            System.Diagnostics.Debug.WriteLine("Mean fitness increase per generation");
            for (int fIndex = 1; fIndex < fitnessHistory.Count; fIndex++)
            {
                //System.Diagnostics.Debug.WriteLine((fIndex - 1).ToString().PadLeft(4, '0') + "->" +
                //    fIndex.ToString().PadLeft(4, '0') + ": " + (fitnessHistory[fIndex] - fitnessHistory[fIndex - 1]));
                System.Diagnostics.Debug.WriteLine((fitnessHistory[fIndex] - fitnessHistory[fIndex - 1]));
            }
        }

        /// <summary>
        /// Returns true if the fitness of the top 5 individuals does not increase after 50 generations 
        /// </summary>
        /// <param name="top5FitnessChangesCtr"></param>
        /// <param name="top5Fitness"></param>
        /// <param name="generation"></param>
        /// <returns></returns>
        private bool TestEarlyAbortOfEvolution(ref int top5FitnessChangesCtr, ref int top5Fitness, int generation)
        {
            int newTop5Fitness = 0;
            for (int i = 0; i < 5; i++)
            {
                newTop5Fitness += Population[i].Fitness;
            }
            if (newTop5Fitness == top5Fitness)
                top5FitnessChangesCtr++;
            top5Fitness = newTop5Fitness;

            if (top5FitnessChangesCtr == 50)
            {
                System.Diagnostics.Debug.WriteLine("Early abort after " + generation + " generations.");
                return true;
            }
            return false;
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

            List<PossibilitiyContainer> possibilities = GetPossibilitiesForCourse(chosenCourse, mutation);
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

        private void PerformRecombination()
        {
            //choose two individuals 
            //produce two new individuals via crossbreeding
            //add to population
        }

        private List<Individual> RouletteSelection(int numberOfIndividualsToChoose)
        {
            //List<Individual> individuals = new List<Individual>();

            //int totalFitness = 0;
            //foreach (Individual i in Population)
            //{
            //    totalFitness += i.Fitness;
            //}

            //int ivoryBall = random.Next(1, totalFitness);
            //for (int numberOfRolls = 0; numberOfRolls < numberOfIndividualsToChoose; numberOfRolls++)
            //{
            //    int currentFitness = 0;
            //    for (int individualIndex = 0; individualIndex < Population.Count; individualIndex++)
            //    {
            //        if ((currentFitness += Population[individualIndex].Fitness) >= ivoryBall)
            //        {
            //            individuals.Add(Population[individualIndex]);
            //            break;
            //        }
            //    }
            //}

            //return individuals;
            return null;
        }



        private Individual CrossBreedIndividuals(Individual individual, Individual individual_2)
        {
            throw new NotImplementedException();
        }





        //private void PMX(Individual parent1, Individual parent2, List<Individual> population)
        //{
        //    int point2 = random.Next(2, maxIndex);
        //    int point1 = random.Next(1, point2 - 1);
        //    List<int> decendent1 = new List<int>();
        //    List<int> decendent2 = new List<int>();

        //    for (int i = 0; i < point1; i++)
        //    {
        //        decendent1.Add(parent1.Occupancy[i]);
        //        decendent2.Add(parent2.Occupancy[i]);
        //    }
        //    for (int i = point1; i < point2; i++)
        //    {
        //        decendent1.Add(parent2.Occupancy[i]);
        //        decendent2.Add(parent1.Occupancy[i]);
        //    }
        //    for (int i = point2; i < parent2.Occupancy.Length; i++)
        //    {
        //        decendent1.Add(parent1.Occupancy[i]);
        //        decendent2.Add(parent2.Occupancy[i]);
        //    }

        //    int index = 0;
        //    for (int i = 0; i < decendent1.Count; i++)
        //    {
        //        if (decendent1[i] != 0)
        //        {
        //            while ((index = decendent1.FindIndex(index + 1, item => item == decendent1[i])) >= 0)
        //            {
        //                if (index != i)
        //                {
        //                    int tmp = decendent1[index];
        //                    decendent1[index] = decendent2[index];
        //                    decendent2[index] = tmp;
        //                }
        //            }
        //        }
        //    }

        //    population.Add(new Individual(decendent1.ToArray()));
        //    population.Add(new Individual(decendent2.ToArray()));
        //}

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
                List<int> courseBlackList = new List<int>();
                Dictionary<int, List<int>> courseBlockStartHours = new Dictionary<int, List<int>>();

                for (int block = 0; block < ttData.Blocks.Length; block++)
                {
                    for (int course = 0; course < individual.Courses.GetLength(0); course++)
                    {
                        if (individual.Courses[course, day, block] != -1)
                        {
                            //Don't consider dummy courses
                            if (!ttData.Courses[course].IsDummy)
                            {
                                //Courses should not appear more than once a day
                                courseBlackList.Add(course);
                                if (courseBlackList.Count<int>(p => p == course) > ttData.Courses[course].NumberOfBlocks)
                                    fitness -= 100;

                                //Courses should start before 13:00
                                fitness += (ttData.Blocks[block].Start.Hour - 13) * -3;

                                //Roompreference
                                if (ttData.Courses[course].RoomPreference != null)
                                {
                                    if (individual.Courses[course, day, block] == ttData.Courses[course].RoomPreference.Index)
                                        fitness += 100;
                                }

                                //A course should not by-pass the lunch break at 13:00
                                //--> Store the start hours
                                if (!courseBlockStartHours.ContainsKey(course))
                                    courseBlockStartHours.Add(course, new List<int>());
                                courseBlockStartHours[course].Add(ttData.Blocks[block].Start.Hour);
                            }
                        }
                    }
                }

                //A course should not by-pass the lunch break at 13:00
                foreach (var key in courseBlockStartHours.Keys)
                {
                    int start = courseBlockStartHours[key][0];
                    int end = courseBlockStartHours[key][courseBlockStartHours[key].Count - 1];
                    if (start - 13 < 0 && !(end - 13 < 0))
                        fitness -= 50;
                }

                List<int> courses = new List<int>();
                //Add all courses of the day that are not flagged as dummy to the course list of the day
                foreach (var course in courseBlackList)
                {
                    if (!courses.Contains(course) && ttData.Courses[course].IsDummy)
                        courses.Add(course);
                }

                //There should be no days with only one course
                if (courses.Count == 1)
                    fitness -= 50;

                //Prefer free days
                if (courses.Count == 0)
                    fitness += 100;
            }
            individual.Fitness = fitness;
        }

        #endregion
    }
}
