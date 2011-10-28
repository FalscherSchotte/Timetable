﻿using System;
using System.Collections.Generic;
using System.Linq;

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
            List<int> fitnessHistory = new List<int>();
            //int top5FitnessChangesCtr = 0;
            //int top5Fitness = 0;
            //int historyChangesCtr = 0;
            
            long start = DateTime.Now.Ticks;

            fitnessHistory.Add(CalcAverageFitness(Population));
            for (CurrentGeneration = 0; CurrentGeneration < numberOfGenerations; CurrentGeneration++)
            {
                List<Individual> individuals = new List<Individual>();
                foreach (Individual individual in Population)
                {
                    //if (random.Next(0, 100) < 15)
                    //{
                    //    List<Individual> selection = RouletteSelection(2);
                    //    Individual[] recombination = PerformRecombination(selection[0], selection[1]);
                    //    individuals.Add(recombination[0]);
                    //    individuals.Add(recombination[1]);
                    //}
                    //else
                    individuals.Add(PerformMutation(individual));
                }
                CalculateFitness(individuals);
                individuals.AddRange(Population);
                SortIndividuals(individuals);
                Population = individuals.GetRange(0, Population.Length).ToArray();

                fitnessHistory.Add(CalcAverageFitness(Population));
                int generationSpan = 10;
                if(fitnessHistory.Count > generationSpan)
                {
                    int averageFitnessChange = 0;
                    for (int i = fitnessHistory.Count - 1; i >= fitnessHistory.Count - generationSpan; i--)
                    {
                        averageFitnessChange += fitnessHistory[i] - fitnessHistory[i - 1];
                    }
                    if (averageFitnessChange / generationSpan <= 1)
                        break;
                }
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

        private bool TestEarlyAbortOfEvolution(ref int top5FitnessChangesCtr, ref int top5Fitness, int generation)
        {
            
            int newTop5Fitness = 0;
            for (int i = 0; i < Math.Min(5, Population.Length); i++)
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

        /// <summary>
        /// Create two new individuals via crossbreeding of two existing individuals
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <returns></returns>
        private Individual[] PerformRecombination(Individual parent1, Individual parent2)
        {
            int crossPoint1 = random.Next(1, parent1.Courses.GetLength(0) - 2);
            int crossPoint2 = random.Next(crossPoint1, parent1.Courses.GetLength(0) - 1);
            Individual combination1 = new Individual(numberOfDays, ttData.Blocks.Length, ttData.Courses.Length,
                ttData.Lecturers.Length, ttData.Rooms.Length, ttData.Groups.Length);
            Individual combination2 = new Individual(numberOfDays, ttData.Blocks.Length, ttData.Courses.Length,
                ttData.Lecturers.Length, ttData.Rooms.Length, ttData.Groups.Length);

            for (int i = 0; i < crossPoint1; i++)
            {
                if (!AdoptSettingOfCourse(i, ref combination1, parent1))
                    return new Individual[] { parent1, parent2 };
                if (!AdoptSettingOfCourse(i, ref combination2, parent2))
                    return new Individual[] { parent1, parent2 };
            }
            for (int i = crossPoint1; i < crossPoint2; i++)
            {
                if (!AdoptSettingOfCourse(i, ref combination1, parent2))
                    return new Individual[] { parent1, parent2 };
                if (!AdoptSettingOfCourse(i, ref combination2, parent1))
                    return new Individual[] { parent1, parent2 };
            }
            for (int i = crossPoint2; i < parent1.Courses.GetLength(0); i++)
            {
                if (!AdoptSettingOfCourse(i, ref combination1, parent1))
                    return new Individual[] { parent1, parent2 };
                if (!AdoptSettingOfCourse(i, ref combination2, parent2))
                    return new Individual[] { parent1, parent2 };
            }

            return new Individual[] { combination1, combination2 };
        }

        private bool AdoptSettingOfCourse(int course, ref Individual targetIndividual, Individual sourceIndividual)
        {
            List<PlacementContainer> placementList = GetPlacementCopyList(course, targetIndividual, sourceIndividual);

            if (placementList.Count > 0)
            {
                foreach (PlacementContainer placement in placementList)
                {
                    targetIndividual.SetChromosome(course, placement.day,
                        placement.block,
                        placement.room,
                        ttData.Courses[course].Group.Index,
                        GetLecturerIndices(course).ToArray());
                }
            }
            else
            {
                List<PlacementContainer> availablePlacements = GetPossibilitiesForCourse(course, targetIndividual);
                if (availablePlacements.Count <= 0)
                    return false;
                int chosenPos = random.Next(0, availablePlacements.Count);
                for (int blockOffset = 0; blockOffset < ttData.Courses[course].NumberOfBlocks; blockOffset++)
                {
                    targetIndividual.SetChromosome(course, availablePlacements[chosenPos].day,
                        availablePlacements[chosenPos].block + blockOffset,
                        availablePlacements[chosenPos].room,
                        ttData.Courses[course].Group.Index,
                        GetLecturerIndices(course).ToArray());
                }
            }

            return true;
        }

        private List<PlacementContainer> GetPlacementCopyList(int course, Individual targetIndividual, Individual sourceIndividual)
        {
            List<PlacementContainer> placementList = new List<PlacementContainer>();
            for (int day = 0; day < sourceIndividual.Courses.GetLength(1); day++)
            {
                for (int block = 0; block < sourceIndividual.Courses.GetLength(2); block++)
                {
                    if (block + ttData.Courses[course].NumberOfBlocks - 1 < sourceIndividual.Courses.GetLength(2))
                    {
                        if (sourceIndividual.Courses[course, day, block] != -1)
                        {
                            int room = sourceIndividual.Courses[course, day, block];
                            if (!IsValidForCourse(course, day, block, room, targetIndividual))
                                return new List<PlacementContainer>();

                            PlacementContainer container = new PlacementContainer();
                            container.day = day;
                            container.block = block;
                            container.room = room;
                            placementList.Add(container);
                        }
                    }
                }
            }
            return placementList;
        }

        private List<Individual> RouletteSelection(int numberOfIndividualsToChoose)
        {
            List<Individual> individuals = new List<Individual>();
            int totalFitness = Population.Sum(p => p.Fitness);
            int ivoryBall = random.Next(1, totalFitness);

            for (int numberOfRolls = 0; numberOfRolls < numberOfIndividualsToChoose; numberOfRolls++)
            {
                int currentFitness = 0;
                for (int individualIndex = 0; individualIndex < Population.Length; individualIndex++)
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
                            fitness += (ttData.Blocks[block].Start.Hour - 13) * 20 * (ttData.Courses[course].IsDummy ? 2 : -1);

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
