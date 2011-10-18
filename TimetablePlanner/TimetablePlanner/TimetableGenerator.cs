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
        private static Random random;
        private static short numberOfDays = 5;
        public Individual[] Population { get; private set; }

        public TimetableGenerator(int numberOfGenerations, int populationSize, TimetableData tableData)
        {
            if (random == null)
                random = new Random(DateTime.Now.Millisecond);
            this.ttData = tableData;
            this.numberOfGenerations = numberOfGenerations;
            CreatePopulation(populationSize);
            SortIndividuals(Population);
        }

        #region Population creation ---------------------------------------------------------------------------------------------

        private void CreatePopulation(int populationSize)
        {
            Population = new Individual[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                Population[i] = new Individual(numberOfDays, (short)ttData.Blocks.Length,
                    (short)ttData.Courses.Length, (short)ttData.Lecturers.Length,
                    (short)ttData.Rooms.Length, (short)ttData.Groups.Length);
                RandomFillIndividual(Population[i]);
            }
            CalculateFitness(Population);
        }

        private void RandomFillIndividual(Individual individual)
        {
            for (short courseIndex = 0; courseIndex < individual.Courses.GetLength(0); courseIndex++)
            {
                for (int courseRepetition = 0; courseRepetition < ttData.Courses[courseIndex].RepeatsPerWeek; courseRepetition++)
                {
                    List<PossibilitiyContainer> posList = GetPossibilitiesForCourse(courseIndex, individual);
                    int chosPos = random.Next(0, posList.Count);
                    for (short blockOffset = 0; blockOffset < ttData.Courses[courseIndex].NumberOfBlocks; blockOffset++)
                    {
                        short day = posList[chosPos].day;
                        short block = posList[chosPos].block;
                        short room = posList[chosPos].room;
                        individual.SetCourse(courseIndex, day, (short)(block + blockOffset), room);
                        for (int lecturerIndex = 0; lecturerIndex < ttData.Courses[courseIndex].Lecturers.Length; lecturerIndex++)
                        {
                            individual.SetLecturer(ttData.Courses[courseIndex].Lecturers[lecturerIndex].Index, day, (short)(block + blockOffset), courseIndex);
                        }
                        individual.SetRoom(room, day, (short)(block + blockOffset), courseIndex);
                        individual.SetGroup(ttData.Courses[courseIndex].Group.Index, day, (short)(block + blockOffset), courseIndex);
                    }
                }
            }
        }

        private struct PossibilitiyContainer
        {
            public short day;
            public short block;
            public short room;
        }

        private List<PossibilitiyContainer> GetPossibilitiesForCourse(short course, Individual individual)
        {
            List<PossibilitiyContainer> possibilities = new List<PossibilitiyContainer>();
            short neededNumberOfBlocks = (short)ttData.Courses[course].NumberOfBlocks;
            for (short day = 0; day < individual.Courses.GetLength(1); day++)
            {
                for (short block = 0; block < individual.Courses.GetLength(2); block++)
                {
                    if (block + neededNumberOfBlocks < individual.Courses.GetLength(2))
                    {
                        for (short room = 0; room < individual.Rooms.GetLength(0); room++)
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

        private bool IsValidForCourse(short course, int day, int block, int room, Individual individual)
        {
            //block available at that day?
            foreach (DayOfWeek exceptionDay in ttData.Blocks[block].Exceptions)
            {
                if ((short)exceptionDay == day)
                    return false;
            }

            //Course already set?
            if (individual.Courses[course, day, block] != -1)
                return false;

            //Room already occupied?
            if (individual.Rooms[room, day, block] != -1)
                return false;

            //Lab?
            if (ttData.Courses[course].NeedsLab != ttData.Rooms[room].IsLab)
                return false;

            //Lecturers available?
            for (int neededLecturer = 0; neededLecturer < ttData.Courses[course].Lecturers.Length; neededLecturer++)
            {
                if (individual.Lecturers[ttData.Courses[course].Lecturers[neededLecturer].Index, day, block] != -1)
                    return false;
            }

            //group available?
            if (individual.Groups[ttData.Courses[course].Group.Index, day, block] != -1)
                return false;

            return true;
        }

        #endregion

        #region Evolution -------------------------------------------------------------------------------------------------------

        public void PerformEvolution()
        {
            for (int generation = 0; generation < numberOfGenerations; generation++)
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
            }
        }

        
        private Individual PerformMutation(Individual individual)
        {
            Individual mutation = Individual.Clone(individual);


            return mutation;
        }

        private void PerformRecombination()
        {
            //choose two individuals 
            //produce two new individuals via crossbreeding
            //add to population
        }

        private void PerformMutation()
        {
            ////choose one individual
            //Individual template = RouletteSelection(1)[0];
            //Individual mutation = template.Clone();

            ////produce a new individual via mutation
            ////-> choose random course and position it new
            //short courseIndex = (short)random.Next(0, ttData.Courses.Length);
            //short courseRepetitionNumber = (short)random.Next(0, ttData.Courses[courseIndex].RepeatsPerWeek);

            //List<int[]> possibilities = GetValidPositionsForCourse(mutation, ttData.Courses[courseIndex]);
            //if (possibilities.Count <= 0)
            //    return;

            //RemoveCourseOccurence(mutation, courseIndex, courseRepetitionNumber);
            //int[] choosenPossibility = possibilities[random.Next(possibilities.Count)];
            //for (int blockCtr = 0; blockCtr < ttData.Courses[courseIndex].NumberOfBlocks; blockCtr++)
            //{
            //    mutation.SetCourseAt(choosenPossibility[0], choosenPossibility[1], choosenPossibility[2] + blockCtr,
            //        courseIndex, ttData.Courses[courseIndex].Lecturers[0].Index);
            //}

            ////add to population
            //Population.Add(mutation);
        }

        private void RemoveCourseOccurence(Individual individual, short courseIndex, short courseRepetitionNumber)
        {
            //int removeAfter = ttData.Courses[courseIndex].NumberOfBlocks * courseRepetitionNumber;
            //int stopBefore = ttData.Courses[courseIndex].NumberOfBlocks * (courseRepetitionNumber + 1);
            //int occurenceCtr = 0;
            //for (int day = 0; day < 5; day++)
            //{
            //    for (int room = 0; room < ttData.Rooms.Length; room++)
            //    {
            //        for (int block = 0; block < ttData.Blocks.Length; block++)
            //        {
            //            if (individual.CourseChromosomes[day][room][block] == courseIndex)
            //            {
            //                if (removeAfter <= occurenceCtr)
            //                {
            //                    individual.CourseChromosomes[day][room][block] = -1;
            //                    foreach (Lecturer lecturer in ttData.Courses[courseIndex].Lecturers)
            //                    {
            //                        individual.LecturerChromosomes[day][room][block][lecturer.Index] = -1;
            //                    }
            //                }
            //                if (stopBefore >= occurenceCtr)
            //                    return;
            //                occurenceCtr++;
            //            }
            //        }
            //    }
            //}
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
            for (short day = 0; day < numberOfDays; day++)
            {
                List<short> courseBlackList = new List<short>();
                for (short block = 0; block < ttData.Blocks.Length; block++)
                {
                    for (short course = 0; course < individual.Courses.GetLength(0); course++)
                    {
                        if (individual.Courses[course, day, block] != -1)
                        {
                            //Courses should not appear more than once a day
                            courseBlackList.Add(course);
                            if (courseBlackList.Count<short>(p => p == course) > ttData.Courses[course].NumberOfBlocks)
                                fitness -= 100;

                            //Courses should start before 13:00
                            fitness += (ttData.Blocks[block].Start.Hour - 13) * -2;

                            //Roompreference
                            if (ttData.Courses[course].RoomPreference != null)
                            {
                                if (individual.Courses[course, day, block] == ttData.Courses[course].RoomPreference.Index)
                                    fitness += 100;
                            }
                        }



                        //Es sollen möglichst keine Tage mit nur einem Kurs existieren


                        //Roompreference fits?
                    }

                    for (int lecturer = 0; lecturer < individual.Lecturers.GetLength(0); lecturer++)
                    {
                        //Freistunden der Lehrer minimal

                    }

                    for (int room = 0; room < individual.Rooms.GetLength(0); room++)
                    {
                    }

                    for (int group = 0; group < individual.Groups.GetLength(0); group++)
                    {
                        //Freistunden der Jahrgänge minimal

                    }
                }
            }
            individual.Fitness = fitness;
        }

        #endregion
    }
}
