using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TimetablePlanner
{
    public class TimetableGenerator
    {
        private TimetableData ttData;
        //private Dictionary<int, DayOfWeek> dayIndices;
        //private Dictionary<int, Block> blockIndices;
        //private Dictionary<int, Room> roomIndices;
        //private int maxIndex;
        private int numberOfGenerations;
        private int populationSize;
        private static Random random;

        public List<Individual> Population { get; private set; }

        public TimetableGenerator(int numberOfGenerations, int populationSize, TimetableData tableData)
        {
            ttData = tableData;
            this.numberOfGenerations = numberOfGenerations;
            this.populationSize = populationSize;
            if (random == null)
                random = new Random(DateTime.Now.Millisecond);

            CreatePopulation();
        }

        #region Population creation ---------------------------------------------------------------------------------------------

        private void CreatePopulation()
        {
            Population = new List<Individual>();
            for (int individualIndex = 0; individualIndex < populationSize; individualIndex++)
            {
                Individual individual = new Individual(CreatePlainCourseChromosomes(), CreatePlainLecturerChromosomes());
                //set a time for each course
                for (int courseIndex = 0; courseIndex < ttData.Courses.Length; courseIndex++)
                {
                    List<int[]> possibilities = GetValidPositionsForCourse(individual, ttData.Courses[courseIndex]);
                    if (possibilities.Count <= 0)
                        throw new Exception("No space for this course available!");

                    //set time for all course instances per week
                    for (int iii = 0; iii < ttData.Courses[courseIndex].RepeatsPerWeek; iii++)
                    {
                        int posIndex = random.Next(possibilities.Count - 1);
                        //occupy all needed blocks
                        for (int blockCtr = 0; blockCtr < ttData.Courses[courseIndex].NumberOfBlocks; blockCtr++)
                        {

                            //TODO: ALL Lecturers of course

                            individual.SetCourseAt(possibilities[posIndex][0], possibilities[posIndex][1],
                                possibilities[posIndex][2] + blockCtr, ttData.Courses[courseIndex].Index, ttData.Courses[courseIndex].Lecturers[0].Index);
                        }
                        possibilities.RemoveAt(posIndex);
                    }
                }
                Population.Add(individual);
            }
            CalculateFitness(Population);
        }

        private List<List<List<List<short>>>> CreatePlainLecturerChromosomes()
        {
            // Days\Rooms\Blocks\Courses\Lecturers\CourseIndex
            List<List<List<List<short>>>> chromosomes = new List<List<List<List<short>>>>();
            for (int dayCtr = 0; dayCtr < 5; dayCtr++)
            {
                chromosomes.Add(new List<List<List<short>>>());
                for (int roomCtr = 0; roomCtr < ttData.Rooms.Length; roomCtr++)
                {
                    chromosomes[dayCtr].Add(new List<List<short>>());
                    for (int blockCtr = 0; blockCtr < ttData.Blocks.Length; blockCtr++)
                    {
                        chromosomes[dayCtr][roomCtr].Add(new List<short>());
                        for (int lecturerCtr = 0; lecturerCtr < ttData.Lecturers.Length; lecturerCtr++)
                        {
                            chromosomes[dayCtr][roomCtr][blockCtr].Add(0);
                        }
                    }
                }
            }
            return chromosomes;
        }

        private List<List<List<short>>> CreatePlainCourseChromosomes()
        {
            // Days\Rooms\Blocks\Courses
            List<List<List<short>>> chromosomes = new List<List<List<short>>>();
            for (int dayCtr = 0; dayCtr < 5; dayCtr++)
            {
                chromosomes.Add(new List<List<short>>());
                for (int roomCtr = 0; roomCtr < ttData.Rooms.Length; roomCtr++)
                {
                    chromosomes[dayCtr].Add(new List<short>());
                    for (int blockCtr = 0; blockCtr < ttData.Blocks.Length; blockCtr++)
                    {
                        chromosomes[dayCtr][roomCtr].Add(0);
                    }
                }
            }
            return chromosomes;
        }

        private List<int[]> GetValidPositionsForCourse(Individual individual, Course course)
        {
            List<int[]> possibilities = new List<int[]>();
            for (int day = 0; day < individual.CourseChromosomes.Count; day++) //Restriction -> Same day for one course instance
            {
                for (int room = 0; room < individual.CourseChromosomes[day].Count; room++)
                {
                    if (course.NeedsLab == course.NeedsLab) //Restriction -> Course with lab restriction
                    {
                        //Restrictions -> Room and lecturer available for the needed number of blocks?
                        //             -> Course must with the whole block size 
                        possibilities.AddRange(GetAvailableBlocksOfSize(individual.CourseChromosomes[day][room],
                            individual.LecturerChromosomes[day][room], course.NumberOfBlocks, day, room));
                    }
                }
            }
            return possibilities;
        }

        private List<int[]> GetAvailableBlocksOfSize(List<short> courseBlockList, List<List<short>> lecturerBlockList, int numberOfBlocks, int day, int room)
        {
            List<int[]> freeBlockIndizes = new List<int[]>();
            for (int blockIndex = 0; blockIndex < courseBlockList.Count; blockIndex++)
            {
                bool free = true;
                for (int neededBlockCtr = 0; neededBlockCtr < numberOfBlocks; neededBlockCtr++)
                {
                    if (blockIndex + neededBlockCtr >= courseBlockList.Count) //Block limit per day reached?
                        free = false;
                    //Lecturer available? 
                    else if (lecturerBlockList[blockIndex + neededBlockCtr][ttData.Courses[courseBlockList[blockIndex + neededBlockCtr]].Lecturers[0].Index] != 0)
                        free = false;


                        //TODO: ALL Lecturers of course!


                    else if (courseBlockList[blockIndex + neededBlockCtr] != 0) //Room available?
                        free = false;

                    if (ttData.Blocks[blockIndex].Exceptions.Length > 0 && free)
                    {
                        //Regard exceptions on blocks
                        for (int exception = 0; exception < ttData.Blocks[blockIndex].Exceptions.Length; exception++)
                        {
                            if (((int)ttData.Blocks[blockIndex].Exceptions[exception]) == day)
                                free = false;
                        }
                    }

                    if (!free)
                        break;
                }
                if (free)
                    freeBlockIndizes.Add(new int[] { day, room, blockIndex }); //day, room, block
            }
            return freeBlockIndizes;
        }

        #endregion

        #region Evolution -------------------------------------------------------------------------------------------------------

        public void PerformEvolution()
        {
            for (int generation = 0; generation < numberOfGenerations; generation++)
            {
                List<Individual> selection = SelectIndividuals();
                for (int i = 0; i < selection.Count; i = i + 2)
                {
                    Population.Add(PMX(selection[i], selection[i + 1]));
                }
                CalculateFitness(Population);
                Population = Population.GetRange(0, populationSize);
            }
        }

        private List<Individual> SelectIndividuals()
        {

            //TODO: Better algorithm (Roulette!)

            List<Individual> selection = new List<Individual>();
            for (int i = 0; i < 10; i++)
            {
                selection.Add(Population[i]);
            }
            return selection;
        }

        private Individual PMX(Individual parent1, Individual parent2)
        {
            Individual child = parent1;


            //TODO

            return child;
        }



        //private void PMX(Individual parent1, Individual parent2, List<Individual> population)
        //{
        //    int point2 = random.Next(2, maxIndex);
        //    int point1 = random.Next(1, point2 - 1);
        //    List<short> decendent1 = new List<short>();
        //    List<short> decendent2 = new List<short>();

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
        //                    short tmp = decendent1[index];
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

        #region Fitness calculation ---------------------------------------------------------------------------------------------

        private void CalculateFitness(List<Individual> individuals)
        {
            foreach (Individual individual in individuals)
            {
                CalculateFitness(individual);
            }
            individuals.Sort(SortByFitness);
        }

        private void CalculateFitness(Individual individual)
        {
            //Bewertungselemente:
            //Freistunden der Lehrer minimal
            //Freistunden der Jahrgänge minimal
            //Es sollen möglichst keine Tage mit nur einem Kurs existieren
            //Keine zwei gleichen Fächer am selben Tag

            int fitness = 0;
            for (int day = 0; day < individual.CourseChromosomes.Count; day++)
            {
                for (int room = 0; room < individual.CourseChromosomes[day].Count; room++)
                {
                    for (int block = 0; block < individual.CourseChromosomes[day][room].Count; block++)
                    {
                        if (individual.CourseChromosomes[day][room][block] != 0)
                        {
                            Course c = ttData.Courses[individual.CourseChromosomes[day][room][block] - 1];

                            //Courses should begin before midday
                            fitness += (ttData.Blocks[block].Start.Hour - 13) * 2;

                            //Reward it when room preference fits
                            if (c.RoomPreference != null && ttData.Rooms[room] == c.RoomPreference)
                                fitness -= 100;


                        }
                    }
                }
            }

            individual.Fitness = -fitness;
        }

        private static int SortByFitness(Individual x, Individual y)
        {
            if (x.Fitness > y.Fitness)
                return -1;
            if (x.Fitness < y.Fitness)
                return +1;
            else
                return 0;
        }

        #endregion
    }
}
