using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TimetablePlanner
{
    public class TimetableGenerator
    {
        private Dictionary<int, DayOfWeek> dayIndices;
        private Dictionary<int, Block> blockIndices;
        private Dictionary<int, Room> roomIndices;
        private int maxIndex;
        private readonly Random random;

        public TimetableGenerator(int numberOfGenerations, int populationSize, TimetableData tableData)
        {
            random = new Random(DateTime.Now.Millisecond);
            List<Individual> population = CreatePopulation(populationSize, tableData);

            foreach (Individual individual in population)
            {
                CalculateFitness(individual, tableData);
            }
            population.Sort(SortByFitness);

            DoTheEvolutionBaby(numberOfGenerations, populationSize, population, tableData);
        }

        private void DoTheEvolutionBaby(int numberOfGenerations, int populationSize, List<Individual> population, TimetableData tableData)
        {
            long start = DateTime.Now.Ticks;
            for (int generation = 0; generation < numberOfGenerations; generation++)
            {
                List<Individual> selection = new List<Individual>();

                //DUMMY
                for (int i = 0; i < 10; i++)
                {
                    selection.Add(population[i]);
                }

                for (int selectionIndex = 0; selectionIndex < selection.Count; selectionIndex = selectionIndex + 2)
                {
                    PMX(selection[selectionIndex], selection[selectionIndex + 1], population);
                }

                foreach (Individual individual in population)
                {
                    CalculateFitness(individual, tableData);
                }
                population.Sort(SortByFitness);
                population.RemoveRange(populationSize, population.Count - populationSize);
            }
            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine(numberOfGenerations + " Generations finished after " + (end - start) / 10000 + "ms");
        }

        private void PMX(Individual parent1, Individual parent2, List<Individual> population)
        {
            int point2 = random.Next(2, maxIndex);
            int point1 = random.Next(1, point2 - 1);
            List<short> decendent1 = new List<short>();
            List<short> decendent2 = new List<short>();

            for (int i = 0; i < point1; i++)
            {
                decendent1.Add(parent1.Occupancy[i]);
                decendent2.Add(parent2.Occupancy[i]);
            }
            for (int i = point1; i < point2; i++)
            {
                decendent1.Add(parent2.Occupancy[i]);
                decendent2.Add(parent1.Occupancy[i]);
            }
            for (int i = point2; i < parent2.Occupancy.Length; i++)
            {
                decendent1.Add(parent1.Occupancy[i]);
                decendent2.Add(parent2.Occupancy[i]);
            }

            int index = 0;
            for (int i = 0; i < decendent1.Count; i++)
            {
                if (decendent1[i] != 0)
                {
                    while ((index = decendent1.FindIndex(index + 1, item => item == decendent1[i])) >= 0)
                    {
                        if (index != i)
                        {
                            short tmp = decendent1[index];
                            decendent1[index] = decendent2[index];
                            decendent2[index] = tmp;
                        }
                    }
                }
            }

            population.Add(new Individual(decendent1.ToArray()));
            population.Add(new Individual(decendent2.ToArray()));
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

        private List<Individual> CreatePopulation(int populationSize, TimetableData tableData)
        {
            long start = DateTime.Now.Ticks;

            //initialize
            List<short> occupancy = new List<short>();
            dayIndices = new Dictionary<int, DayOfWeek>();
            blockIndices = new Dictionary<int, Block>();
            roomIndices = new Dictionary<int, Room>();
            int ctr = 0;
            for (int dayCtr = 0; dayCtr < 5; dayCtr++)
            {
                for (int blockCtr = 0; blockCtr < tableData.Blocks.Length; blockCtr++)
                {
                    for (int roomCtr = 0; roomCtr < tableData.Rooms.Length; roomCtr++)
                    {
                        occupancy.Add(0);
                        dayIndices.Add(ctr, (DayOfWeek)(dayCtr + 1));
                        blockIndices.Add(ctr, tableData.Blocks[blockCtr]);
                        roomIndices.Add(ctr, tableData.Rooms[roomCtr]);
                        ctr++;
                    }
                }
            }
            maxIndex = ctr - 1;

            List<Individual> population = new List<Individual>();
            for (int iii = 0; iii < populationSize; iii++)
            {
                short[] occupancyCopy = new short[occupancy.Count];
                occupancy.CopyTo(occupancyCopy);

                //random fill courses for individual start values
                List<short> courseIndizes = new List<short>();
                foreach (Course c in tableData.Courses)
                {
                    courseIndizes.Add(c.Index);
                }
                //TODO: Optimize loop -> bottleneck when bad series of random numbers occure
                while (courseIndizes.Count > 0)
                {
                    int index = random.Next(maxIndex);
                    while (occupancyCopy[index] != 0)
                    {
                        index = random.Next(maxIndex);
                    }
                    occupancyCopy[index] = courseIndizes[courseIndizes.Count - 1];
                    courseIndizes.RemoveAt(courseIndizes.Count - 1);
                }

                population.Add(new Individual(occupancyCopy));
            }

            long end = DateTime.Now.Ticks;
            System.Diagnostics.Debug.WriteLine("Time to create population of size " + populationSize + ": " + (end - start) / 10000 + "ms");
            return population;
        }

        private class Individual
        {
            public int Fitness { get; set; }
            public short[] Occupancy { get; set; }

            public Individual(short[] occupancyCopy)
            {
                this.Occupancy = occupancyCopy;
            }

            public void switchElements(int index1, int index2)
            {
                short tmp = this.Occupancy[index1];
                this.Occupancy[index1] = this.Occupancy[index2];
                this.Occupancy[index2] = tmp;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Fitness + ": ");
                for (int i = 0; i < Occupancy.Length; i++)
                {
                    builder.Append(String.Format("{0:000}", Occupancy[i]) + (i == Occupancy.Length - 1 ? "" : ","));
                }
                return builder.ToString();
            }
        }

        private void CalculateFitness(Individual individual, TimetableData data)
        {
            int fitness = 10000;

            //Bewertungselemente:
            //Keine zwei gleichen Fächer am selben Tag
            //Freistunden der Lehrer minimal
            //Freistunden der Jahrgänge minimal
            //Beachten wie Dozenten verfügbar sind
            //Kurse nur an bestimmten Tagen
            //Es sollen möglichst keine Tage mit nur einem Kurs existieren
            //Keine Kurse an Vorlesungsausnahmen

            DateTime midDay = DateTime.Parse("13:00");
            for (int i = 0; i < individual.Occupancy.Length; i++)
            {
                if (individual.Occupancy[i] > 0)
                {
                    Course c = data.Courses[individual.Occupancy[i] - 1];
                    
                    //Courses should beginn before 01:00 pm
                    if (blockIndices[i].Start > midDay)
                        fitness--;

                    //Reduce fitness for courses that are not in the correct room type
                    if (c.NeedsLab != roomIndices[i].IsLab)
                        fitness = fitness - 100;

                    //Increase fitness if roompreference fits
                    if (c.RoomPreference == roomIndices[i])
                        fitness = fitness + 10;
                }
            }

            


            individual.Fitness = fitness;
        }


    }
}
