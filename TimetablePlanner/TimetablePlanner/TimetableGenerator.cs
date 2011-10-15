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
                Population.Add(individual);

                //set a time for each course
                for (int courseIndex = 0; courseIndex < ttData.Courses.Length; courseIndex++)
                {
                    //set time for all course instances per week
                    for (int iii = 0; iii < ttData.Courses[courseIndex].RepeatsPerWeek; iii++)
                    {
                        List<int[]> possibilities = GetValidPositionsForCourse(individual, ttData.Courses[courseIndex]);
                        if (possibilities.Count <= 0)
                            throw new Exception("No space for this course available!");
                        
                        int posIndex = random.Next(possibilities.Count);
                        //occupy all needed blocks
                        for (int blockCtr = 0; blockCtr < ttData.Courses[courseIndex].NumberOfBlocks; blockCtr++)
                        {
                            individual.SetCourseAt(possibilities[posIndex][0], possibilities[posIndex][1],
                                possibilities[posIndex][2] + blockCtr, ttData.Courses[courseIndex].Index, ttData.Courses[courseIndex].Lecturers[0].Index);
                        }
                    }
                }
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
                            chromosomes[dayCtr][roomCtr][blockCtr].Add(-1);
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
                        chromosomes[dayCtr][roomCtr].Add(-1);
                    }
                }
            }
            return chromosomes;
        }

        /// <summary>
        /// Returns a list of all valid positions for that course
        /// </summary>
        /// <param name="individual">Individual to use</param>
        /// <param name="course">Course to inspect</param>
        /// <returns>list of the valid positions</returns>
        private List<int[]> GetValidPositionsForCourse(Individual individual, Course course)
        {
            List<int[]> possibilities = new List<int[]>();

            //Restriction -> Same day for one course instance
            for (int day = 0; day < individual.CourseChromosomes.Count; day++)
            {
                //Restriction -> A course does not change rooms when taking more time than one block 
                for (int room = 0; room < individual.CourseChromosomes[day].Count; room++)
                {
                    //Restriction -> Course with lab restriction
                    if (ttData.Rooms[room].IsLab == course.NeedsLab)
                    {
                        possibilities.AddRange(GetAvailableBlocksOfSize(individual.CourseChromosomes[day][room],
                            individual.LecturerChromosomes[day][room],
                            course.NumberOfBlocks, course.Lecturers, course.Group,
                            day, room));
                    }
                }
            }
            return possibilities;
        }

        /// <summary>
        /// Returns a list of indizes of the blocks that are not occupied
        /// </summary>
        /// <param name="blockListCourses"></param>
        /// <param name="blockListLecturers"></param>
        /// <param name="neededBlocks"></param>
        /// <param name="dayIndex"></param>
        /// <param name="roomIndex"></param>
        /// <returns>List of int-Arrays (day, room, block) that are meet the requirements</returns>
        private List<int[]> GetAvailableBlocksOfSize(List<short> blockListCourses, List<List<short>> blockListLecturers,
            int neededBlocks, Lecturer[] neededLecturers, Group neededGroup, int dayIndex, int roomIndex)
        {
            List<int[]> freeBlockIndizes = new List<int[]>();
            for (int blockIndex = 0; blockIndex < blockListCourses.Count; blockIndex++)
            {
                for (int neededBlockCtr = 0; neededBlockCtr < neededBlocks; neededBlockCtr++)
                {
                    if (BlockMeetsRequirements(blockListCourses, blockListLecturers,
                        dayIndex, roomIndex, blockIndex + neededBlockCtr,
                        neededBlocks, neededLecturers, neededGroup))
                        freeBlockIndizes.Add(new int[] { dayIndex, roomIndex, blockIndex }); //day, room, block
                }
            }
            return freeBlockIndizes;
        }

        /// <summary>
        /// Checks if the block meets the requirements
        /// </summary>
        /// <param name="blockListCourses">List of blocks of that specific day in that specific room with course number as content</param>
        /// <param name="blockListLecturers">List of blocks of that day and room with the list of all lecturers as content (value is the index of the course they are lecturing)</param>
        /// <param name="neededBlocks">Number of blocks the course needs</param>
        /// <param name="neededLecturers">Array of lecturers the course needs</param>
        /// <param name="blockIndex">Blockindex to inspect</param>
        /// <returns>block fits true/false</returns>
        private bool BlockMeetsRequirements(List<short> blockListCourses, List<List<short>> blockListLecturers,
            int dayIndex, int roomIndex, int blockIndex,
            int neededBlocks, Lecturer[] neededLecturers, Group neededGroup)
        {
            //does course fit?
            if (blockIndex + neededBlocks >= blockListCourses.Count)
                return false;

            //Requirements must fit for each of the needed blocks
            for (int neededBlockCtr = 0; neededBlockCtr < neededBlocks; neededBlockCtr++)
            {
                ////group of course already occupied in that block?
                //if (blockListCourses[blockIndex + neededBlockCtr] != -1)
                //{
                //    if (neededGroup.Id.Equals(ttData.Courses[blockListCourses[blockIndex + neededBlockCtr]].Group.Id))
                //        return false;
                //}

                //are the needed lecturers available?
                foreach (Lecturer lecturer in neededLecturers)
                {
                    if (blockListLecturers[blockIndex + neededBlockCtr][lecturer.Index] != -1)
                        return false;
                }

                //is the room available?
                if (blockListCourses[blockIndex + neededBlockCtr] != -1)
                    return false;

                //is that block an exception?
                foreach (DayOfWeek day in ttData.Blocks[blockIndex + neededBlockCtr].Exceptions)
                {
                    if ((int)day == dayIndex)
                        return false;
                }
            }
            return true;
        }

        #endregion

        #region Evolution -------------------------------------------------------------------------------------------------------

        public void PerformEvolution()
        {
            for (int generation = 0; generation < numberOfGenerations; generation++)
            {
                //if (random.Next(1, 2) <= 1)
                PerformMutation();
                //else
                //    PerformRecombination();

                CalculateFitness(Population);
                Population = Population.GetRange(0, populationSize);
            }
        }

        private void PerformRecombination()
        {
            //choose two individuals 
            //produce two new individuals via crossbreeding
            //add to population
        }

        private void PerformMutation()
        {
            //choose one individual
            Individual template = RouletteSelection(1)[0];
            Individual mutation = template.Clone();

            //produce a new individual via mutation
            //-> choose random course and position it new
            short courseIndex = (short)random.Next(0, ttData.Courses.Length);
            short courseRepetitionNumber = (short)random.Next(0, ttData.Courses[courseIndex].RepeatsPerWeek);

            List<int[]> possibilities = GetValidPositionsForCourse(mutation, ttData.Courses[courseIndex]);
            if (possibilities.Count <= 0)
                return;

            RemoveCourseOccurence(mutation, courseIndex, courseRepetitionNumber);
            int[] choosenPossibility = possibilities[random.Next(possibilities.Count)];
            for (int blockCtr = 0; blockCtr < ttData.Courses[courseIndex].NumberOfBlocks; blockCtr++)
            {
                mutation.SetCourseAt(choosenPossibility[0], choosenPossibility[1], choosenPossibility[2] + blockCtr,
                    courseIndex, ttData.Courses[courseIndex].Lecturers[0].Index);
            }

            //add to population
            Population.Add(mutation);
        }

        private void RemoveCourseOccurence(Individual individual, short courseIndex, short courseRepetitionNumber)
        {
            int removeAfter = ttData.Courses[courseIndex].NumberOfBlocks * courseRepetitionNumber;
            int stopBefore = ttData.Courses[courseIndex].NumberOfBlocks * (courseRepetitionNumber + 1);
            int occurenceCtr = 0;
            for (int day = 0; day < 5; day++)
            {
                for (int room = 0; room < ttData.Rooms.Length; room++)
                {
                    for (int block = 0; block < ttData.Blocks.Length; block++)
                    {
                        if (individual.CourseChromosomes[day][room][block] == courseIndex)
                        {
                            if (removeAfter <= occurenceCtr)
                            {
                                individual.CourseChromosomes[day][room][block] = -1;
                                foreach (Lecturer lecturer in ttData.Courses[courseIndex].Lecturers)
                                {
                                    individual.LecturerChromosomes[day][room][block][lecturer.Index] = -1;
                                }
                            }
                            if (stopBefore >= occurenceCtr)
                                return;
                            occurenceCtr++;
                        }
                    }
                }
            }
        }

        private List<Individual> RouletteSelection(int numberOfIndividualsToChoose)
        {
            List<Individual> individuals = new List<Individual>();

            int totalFitness = 0;
            foreach (Individual i in Population)
            {
                totalFitness += i.Fitness;
            }

            int ivoryBall = random.Next(1, totalFitness);
            for (int numberOfRolls = 0; numberOfRolls < numberOfIndividualsToChoose; numberOfRolls++)
            {
                int currentFitness = 0;
                for (int individualIndex = 0; individualIndex < Population.Count; individualIndex++)
                {
                    if ((currentFitness += Population[individualIndex].Fitness) >= ivoryBall)
                    {
                        individuals.Add(Population[individualIndex]);
                        break;
                    }
                }
            }

            return individuals;
        }



        private Individual CrossBreedIndividuals(Individual individual, Individual individual_2)
        {
            throw new NotImplementedException();
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
                        if (individual.CourseChromosomes[day][room][block] != -1)
                        {
                            Course c = ttData.Courses[individual.CourseChromosomes[day][room][block]];

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
