using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{

    public int input_layer_size;
    public int hidden_layer_size;
    public int output_layer_size;
    public int num_hidden_layers;

    public List<Layer> layers;

    public float learning_rate = 0.1f;

    public NeuralNetwork(int input_layer_size, int hidden_layer_size, int output_layer_size, int num_hidden_layers, Matrix input_vector){
        this.input_layer_size = input_layer_size;
        this.hidden_layer_size = hidden_layer_size;
        this.output_layer_size = output_layer_size;
        this.num_hidden_layers = num_hidden_layers;


        setupNetwork(input_layer_size, hidden_layer_size, output_layer_size, num_hidden_layers, input_vector);
    }

    public class Layer{
        public Layer(int size, int network_pos, List<Matrix> weights, Matrix biases, Matrix values){
            this.size = size;
            this.network_pos = network_pos;
            this.weights = weights;
            this.biases = biases;
            this.values = values;
        }

        public int size;
        public int network_pos;
        public List<Matrix> weights;
        public Matrix biases;
        public Matrix values;

    }

    private void setupNetwork(int input_layer_size, int hidden_layer_size, int output_layer_size, int num_hidden_layers, Matrix input_vector){
        layers = new List<Layer>();
        for (int i = 0; i < num_hidden_layers + 2; i++){
            // Matrix weights;
            List<Matrix> weights = new List<Matrix>();
            Matrix biases;
            Matrix values;

            if (i == 0){ //Input layers
                layers.Add(new Layer(input_layer_size, i, null, new Matrix(0, 0), input_vector));
            }
            else if (i == (num_hidden_layers + 2) - 1){ //Output layer
                for(int j = 0; j < output_layer_size; j++){
                    Matrix tmp = new Matrix(layers[i-1].size, 1);
                    tmp.fillRandom(-1.0f, 1.0f);
                    weights.Add(tmp);
                }
                // weights = new Matrix(layers[i-1].size, 1);
                biases = new Matrix(output_layer_size, 1);
                // weights.fillRandom(-1.0f, 1.0f);
                biases.fillRandom(-1.0f, 1.0f);
                values = new Matrix(output_layer_size, 1);
                values.fillZeros();
                layers.Add(new Layer(input_layer_size, i, weights, biases, values));
            }
            else { //Hidden layers

                for(int j = 0; j < hidden_layer_size; j++){
                    Matrix tmp = new Matrix(layers[i-1].size, 1);
                    tmp.fillRandom(-1.0f, 1.0f);
                    weights.Add(tmp);
                }
                // weights = new Matrix(layers[i-1].size, 1);
                biases = new Matrix(hidden_layer_size, 1);
                // weights.fillRandom(-1.0f, 1.0f);
                biases.fillRandom(-1.0f, 1.0f);
                values = new Matrix(hidden_layer_size, 1);
                values.fillZeros();
                layers.Add(new Layer(hidden_layer_size, i, weights, biases, values));
            }
        }
    }

    // public Matrix activate(Matrix weights, Matrix biases, Matrix values){
    public Matrix activate(List<Matrix> weights, Matrix biases, Matrix values){
        Matrix tmp = new Matrix(biases.vectorSize(), 1);
        for (int i = 0; i < tmp.vectorSize(); i++){
            tmp.insert(weights[i].vectorInnerProd(values), i);
        }
        tmp = tmp + biases;
        // Matrix mat = (weights.vectorInnerProd(values)) + biases;
        // return mat.sigmoid();
        return tmp.sigmoid();
    }

    public Matrix forward_propagate(){
        for (int i = 0; i < layers.Count; i++){
            if (layers[i].network_pos == 0){ //Ignore input layer
                continue;
            }

            Matrix act_values = activate(layers[i].weights, layers[i].biases, layers[i - 1].values);
            // act_values.printMatrix();
            layers[i].values = act_values;
        }

        return layers[layers.Count - 1].values;
    }

    public void refreshInput(Matrix input){
        layers[0].values = input;
        for (int i = 1; i < layers.Count; i++){
            layers[i].values.fillZeros();
        }
    }

    public int getPrediction(Matrix outputs){
        float currentMax = -1;
        int currentIndex = -1;
        for (int i = 0; i < outputs.rows; i++){
            if (outputs.get(i) > currentMax){
                currentMax = outputs.get(i);
                currentIndex = i;
            }
        }

        return currentIndex;
    }

    public void mutate(NeuralNetwork parent1, NeuralNetwork parent2){
        for(int i = 1; i < layers.Count; i++){ //Loop through layers
            for (int j = 0; j < layers[i].weights.Count / 2; j++){ //Loop through first half of weights of layer
                layers[i].weights[j] = parent1.layers[i].weights[j];
            }

            for (int k = layers[i].weights.Count / 2; k < layers[i].weights.Count; k++){ //Loop through first half of weights of layer
                layers[i].weights[k] = parent1.layers[i].weights[k];
            }
        }
    }

    public Matrix train(Matrix input){
        refreshInput(input);
        Matrix outputs = forward_propagate();

        return outputs;
    }
}
