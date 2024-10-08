using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        SixState[] populationStates; 

        int moveCounter;
        int population;
        int crossingPoint;
        int probabilityExponent; 

        double mutationRate;
        int[] heuristicTable;
        ArrayList bestMoves;
        object chosenMove;

        public Form1() {
            InitializeComponent();

            population = 10;
            crossingPoint = n / 2;
            mutationRate = 0.5;
            probabilityExponent = 5;

            side = pictureBox1.Width / n;

            populationStates = new SixState[population];
            populationStates[0] = startState = RandomSixState();
            for (int i = 1; i < population; i++)
                populationStates[i] = RandomSixState();

            label3.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generations: " + moveCounter;
            UpdateUI();
            label1.Text = "Attacking pairs: " + GetAttackingPairs(startState);
        }

        private void SortPopulation() {
            (SixState, int)[] statesWithHeuristic = new (SixState, int)[population];

            for (int i = 0; i < population; i++)
                statesWithHeuristic[i] = (populationStates[i], heuristicTable[i]);

            Array.Sort(statesWithHeuristic, (x, y) => x.Item2.CompareTo(y.Item2));

            for (int i = 0; i < population; i++)
            {
                populationStates[i] = statesWithHeuristic[i].Item1;
                heuristicTable[i] = statesWithHeuristic[i].Item2;
            }
        }

        private SixState[] GenerateChildren(SixState parent1, SixState parent2) {
            SixState[] children = new SixState[2];

            children[0] = new SixState(parent2);
            children[1] = new SixState(parent1);

            for (int i = 0; i < n / 2; i++)
            {
                children[0].Y[i] = parent2.Y[i];
                children[1].Y[i] = parent1.Y[i];
            }

            for (int i = n / 2; i < n; i++)
            {
                children[0].Y[i] = parent1.Y[i];
                children[1].Y[i] = parent2.Y[i];
            }

            Random rand = new Random();
            if (rand.NextDouble() <= mutationRate)
                children[0].Y[rand.Next(0, 6)] = rand.Next(0, 6);

            if (rand.NextDouble() <= mutationRate)
                children[1].Y[rand.Next(0, 6)] = rand.Next(0, 6);

            return children;
        }

        private void Repopulate() {
            SixState[] newPopulation = new SixState[population];
            SixState[] children = GenerateChildren(populationStates[0], populationStates[1]);

            newPopulation[0] = new SixState(children[0]);
            newPopulation[1] = new SixState(children[1]);

            for (int i = 3; i < population; i += 2)
            {
                int[] parentIndices = GetParentsIndex();
                children = GenerateChildren(populationStates[parentIndices[0]], populationStates[parentIndices[1]]);
                newPopulation[i] = new SixState(children[0]);
                newPopulation[i - 1] = new SixState(children[1]);
            }

            populationStates = newPopulation;
        }

        private int[] GetParentsIndex() {
            SortPopulation();

            int[] parents = new int[2];
            Random rand = new Random();

            for (int i = 0; i < 2; i++)
            {
                parents[i] = (int)(population * Math.Pow(rand.NextDouble(), probabilityExponent));
            }

            return parents;
        }

        private void UpdateUI() {
            heuristicTable = GetHeuristicTableForPossibleMoves(populationStates);
            bestMoves = GetBestMoves(heuristicTable);

            listBox1.Items.Clear();
            foreach (Point move in bestMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bestMoves.Count > 0)
                chosenMove = ChooseMove(bestMoves);

            label2.Text = "Chosen parent index: " + 0;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }

                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e) {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }

                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState RandomSixState() {
            Random rand = new Random();
            return new SixState(rand.Next(n),
                                rand.Next(n),
                                rand.Next(n),
                                rand.Next(n),
                                rand.Next(n),
                                rand.Next(n));
        }

        private int GetAttackingPairs(SixState state) {
            int attackers = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (state.Y[i] == state.Y[j])
                        attackers++;

                    if (state.Y[j] == state.Y[i] + j - i)
                        attackers++;

                    if (state.Y[i] == state.Y[j] + j - i)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[] GetHeuristicTableForPossibleMoves(SixState[] states) {
            int[] heuristics = new int[population];

            for (int i = 0; i < population; i++)
                heuristics[i] = GetAttackingPairs(states[i]);

            return heuristics;
        }

        private ArrayList GetBestMoves(int[] heuristicTable) {
            ArrayList bestMovesList = new ArrayList();

            SortPopulation();
            int bestHeuristic = heuristicTable[0];

            for (int i = 1; i < population; i++)
                if (heuristicTable[0] == heuristicTable[i])
                    bestMovesList.Add(new Point(i, 0));

            label5.Text = "Possible Moves (H=" + bestHeuristic + ")";
            return bestMovesList;
        }

        private object ChooseMove(ArrayList moves) {
            return moves[0];
        }

        private void ExecuteMove() {
            Repopulate();
            moveCounter++;

            UpdateUI();

            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = populationStates[0].Y[i];
            }

            pictureBox2.Refresh();
        }

        private void button1_Click(object sender, EventArgs e) {
            label3.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generations: " + moveCounter;

            if (GetAttackingPairs(startState) > 0)
                ExecuteMove();

            label1.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generations: " + moveCounter;
        }

        private void button3_Click(object sender, EventArgs e) {
            startState = RandomSixState();
            populationStates[0] = new SixState(startState);
            for (int i = 1; i < population; i++)
                populationStates[i] = new SixState(startState);

            moveCounter = 0;

            label3.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generations: " + moveCounter;

            UpdateUI();

            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + GetAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e) {
            label3.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generations: " + moveCounter;

            while (GetAttackingPairs(startState) > 0)
                ExecuteMove();

            label3.Text = "Attacking pairs: " + GetAttackingPairs(startState);
            label4.Text = "Generationss