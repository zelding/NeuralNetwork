﻿
public class EntityState : EntityInfo
{
    string Name;
    string NeuStr;

    NeuralNetwork Brain;
    Genes Genes;

    float Energy;
    float Age ;
    float Distance;
    float Consumption;
    float topSpeed;

    public EntityState( EntityController entity )
    {
        Brain = new NeuralNetwork(entity.Brain, true);
        Genes = new Genes(entity.Genes, true);

        Energy      = 0;
        Name        = entity.Name;
        NeuStr      = Brain.lineage;
        Age         = entity.Age;
        Distance    = entity.Distance;
        Consumption = entity.Consumption;
        topSpeed    = entity.GetTopSpeed();
    }

    #region EntityInfo

    public string GetName(){
        return Name;
    }

    public string GetNeuronString() {
        return NeuStr;
    }

    public NeuralNetwork GetBrain() {
        return Brain;
    }

    public Genes GetGenes()
    {
        return Genes;
    }

    public float GetEnergy() {
        return Energy;
    }

    public float GetAge() {
        return Age;
    }

    public float GetDistance() {
        return Distance;
    }

    public float GetConsumption() {
        return Consumption;
    }

    public bool isAlive() {
        return false;
    }

    public float GetFittness() {
        return (Age + Distance) * Consumption;
    }

    public float GetTopSpeed() {
        return topSpeed;
    }

    #endregion
}

public interface EntityInfo {
    string GetName();

    string GetNeuronString();

    NeuralNetwork GetBrain();

    Genes GetGenes();

    float GetEnergy();
    float GetAge();
    float GetDistance();
    float GetConsumption();
    bool  isAlive();
    float GetFittness();
    float GetTopSpeed();
}