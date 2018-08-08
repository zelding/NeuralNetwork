using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public enum NodeTypes {Input, Hidden, Output, Bias};

    public readonly NodeTypes type;

    protected List<Connection> Connections;

    private float value;

    private float previousValue;

    private uint innovation = 0;

    public Node(NodeTypes type)
    {
        value = 0;
        previousValue = value;
        innovation = 0;

        Connections = new List<Connection>();

        this.type = type;
    }

    public Node(Node n)
    {
        value = n.value;
        previousValue = 0;
        innovation = n.innovation;

        Connections = new List<Connection>(n.Connections);

        type = n.type;
    }

    public void SetValue(float v)
    {
        previousValue = value;
        value = v;
    }

    public List<Connection> GetConnections()
    {
        return Connections;
    }
}
