using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RBF
{
    class Network
    {
        private double[,] associativeLayer_weights;
        private double[] associativeLayer_thresholds;

        private double[,] outputLayer_weights;
        private double[] outputLayer_thresholds;

        private int sensorCount;
        private int outputCount;
        private int associativeCount;

        public double alfa = 1.2;
        public double beta=0.9;
        public double maxErrorTreshold=0.05;

        public int iterationCount { get; private set; }

        private string[] classesNames;
        public Network(Dictionary<string, List<int[]>> classes)
        {
            int size = classes.First().Value.First().Length;

            int i = 0;
            classesNames = new string[classes.Count];
            foreach (KeyValuePair<string, List<int[]>> tmp in classes)
                classesNames[i++] = tmp.Key;

            iterationCount = TeachNeuralNetwork(classes);
        }

        private int TeachNeuralNetwork(Dictionary<string, List<int[]>> classes)//обучение
        {
            sensorCount = classes.First().Value.First().Length;
            outputCount = classes.Count;

            associativeCount = (int)Math.Sqrt(sensorCount / outputCount); //(sensorCount + outputCount) / 2;

            associativeLayer_weights = new double[sensorCount, associativeCount];
            associativeLayer_thresholds = new double[associativeCount];

            outputLayer_weights = new double[associativeCount, outputCount];
            outputLayer_thresholds = new double[outputCount];

            Randomize();

            double[] associativeNeurons = new double[associativeCount];
            double[] outputNeurons = new double[outputCount];

            List<double> maxErrors = new List<double>();

            int iterCount = 0;
            double errorSumPrev = double.MaxValue;

            double[] outputErrors = new double[outputCount];
            double[] outputErrorsAbs = new double[outputCount];

            while (true)
            {
                maxErrors.Clear();
                int classNumber = 0;

                foreach (KeyValuePair<string, List<int[]>> tmp in classes)
                {

                    List<int[]> images = tmp.Value;
                   

                    foreach (int[] img in images)
                    {
                        for (int j = 0; j < associativeCount; j++)
                        {
                            double sum = 0;
                            for (int i = 0; i < sensorCount; i++)
                                sum += associativeLayer_weights[i, j] * img[i];

                            associativeNeurons[j] = ActivationFunction(sum + associativeLayer_thresholds[j]);
                        }

                        for (int k = 0; k < outputCount; k++)
                        {
                            double sum = 0;
                            for (int j = 0; j < associativeCount; j++)
                                sum += outputLayer_weights[j, k] * associativeNeurons[j];

                            outputNeurons[k] = ActivationFunction(sum + outputLayer_thresholds[k]);
                        }


                        //---------------------------------------------------
                        Array.Clear(outputErrors, 0, outputErrors.Length);
                        outputErrors[classNumber] = 1;

                        for (int k = 0; k < outputCount; k++)
                            outputErrors[k] -= outputNeurons[k];

                        outputErrorsAbs = outputErrors.Select(x => Math.Abs(x)).ToArray();
                        maxErrors.Add(outputErrorsAbs.Max());

                        double[] associativeErrors = new double[associativeCount];
                        for (int j = 0; j < associativeCount; j++)
                        {
                            double sum = 0;
                            for (int k = 0; k < outputCount; k++)
                                sum += outputErrors[k] * outputNeurons[k] * (1 - outputNeurons[k]) * outputLayer_weights[j, k];

                            associativeErrors[j] = sum;
                        }

                        //-----------------------------------------------------------------------------
                        // Correct weights and tresholds

                        for (int j = 0; j < associativeCount; j++)
                        {
                            for (int k = 0; k < outputCount; k++)
                            {
                                outputLayer_weights[j, k] +=
                                    alfa * outputNeurons[k] * (1 - outputNeurons[k]) * outputErrors[k] * associativeNeurons[j];
                            }
                        }
                        for (int k = 0; k < outputCount; k++)
                        {
                            outputLayer_thresholds[k] +=
                                alfa * outputNeurons[k] * (1 - outputNeurons[k]) * outputErrors[k];
                        }

                        for (int i = 0; i < sensorCount; i++)
                        {
                            for (int j = 0; j < associativeCount; j++)
                            {
                                associativeLayer_weights[i, j] +=
                                    beta * associativeNeurons[j] * (1 - associativeNeurons[j]) * associativeErrors[j] * img[i];
                            }
                        }
                        for (int j = 0; j < associativeCount; j++)
                        {
                            associativeLayer_thresholds[j] +=
                                beta * associativeNeurons[j] * (1 - associativeNeurons[j]) * associativeErrors[j];
                        }
                    }
                    classNumber++;
                    
                }
               
                iterCount++;

                double maxError = maxErrors.Max();
                if (maxError < maxErrorTreshold)
                    break;

                double errorSum = outputErrorsAbs.Sum();
                if (errorSum >= errorSumPrev)
                {
                    Randomize();
                    errorSumPrev = double.MaxValue;
                }
                else
                    errorSumPrev = errorSum;
            }
            return iterCount;
        }

        private double ActivationFunction(double x)//Ф-ЦИЯ АКТИВАЦИИ
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        private void Randomize()
        {
            Random random = new Random(DateTime.Now.Day ^ Environment.TickCount);

            for (int i = 0; i < associativeLayer_weights.GetLength(0); i++)
            {
                for (int j = 0; j < associativeLayer_weights.GetLength(1); j++)
                    associativeLayer_weights[i, j] = 2 * random.NextDouble() - 1;
            }

            for (int i = 0; i < associativeLayer_thresholds.Length; i++)
                associativeLayer_thresholds[i] = 2 * random.NextDouble() - 1;

            for (int i = 0; i < outputLayer_weights.GetLength(0); i++)
            {
                for (int j = 0; j < outputLayer_weights.GetLength(1); j++)
                    outputLayer_weights[i, j] = 2 * random.NextDouble() - 1;
            }

            for (int i = 0; i < outputLayer_thresholds.Length; i++)
                outputLayer_thresholds[i] = 2 * random.NextDouble() - 1;
        }

        public Dictionary<string, double> ClassifyImage(int[] image)
        {
            if (sensorCount != image.Length)
                throw new Exception("Image has wrong size.");

            double[] associativeNeurons = new double[associativeCount];
            double[] outputNeurons = new double[outputCount];

            for (int j = 0; j < associativeCount; j++)
            {
                double sum = 0;
                for (int i = 0; i < sensorCount; i++)
                    sum += associativeLayer_weights[i, j] * image[i];

                associativeNeurons[j] = ActivationFunction(sum + associativeLayer_thresholds[j]);
            }

            for (int k = 0; k < outputCount; k++)
            {
                double sum = 0;
                for (int j = 0; j < associativeCount; j++)
                    sum += outputLayer_weights[j, k] * associativeNeurons[j];

                outputNeurons[k] = ActivationFunction(sum + outputLayer_thresholds[k]);
            }

            Dictionary<string, double> result = new Dictionary<string, double>();
            int t = 0;
            foreach (string className in classesNames)
            {
                result.Add(classesNames[t], outputNeurons[t]);
                t++;
            }

            return result;
        }
    
    
}
}
