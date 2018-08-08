
public class Connection {

    protected Node input;

    protected Node output;

    protected float weight = 0f;

    private readonly bool enabled = false;

    private uint innovation = 0;

    public Connection(Node input, Node output, float weight)
    {
        if (input.type == Node.NodeTypes.Output || output.type == Node.NodeTypes.Input || output.type == Node.NodeTypes.Bias) {
            throw new System.InvalidOperationException("Input/output node type invalid");
        }

        this.input = input;
        this.output = output;
        this.weight = weight;

        innovation = 0;
        enabled = true;
    }

    public static Connection FindBetween(Connection[] connections, Node input, Node output)
    {
        for(uint i = 0; i < connections.Length; i++) {
            if ( connections[i].input == input && connections[i].output == output) {
                return connections[i];
            }
        }

        return null;
    }

    public static bool IsCyclic(Connection c, Node input)
    {
        if ( !c.enabled ) {
            return false;
        }

        if (input.type == Node.NodeTypes.Input || input.type == Node.NodeTypes.Bias || c.output.type == Node.NodeTypes.Output) {
            return false;
        }

        Connection current = c;

        while (current.output.type != Node.NodeTypes.Output) { // r u sure its going out always?

            if (input == current.output) {
                return true;
            }

            for (int i = 0; i < current.output.GetConnections().Count; i++) { 
                if ( current.output.GetConnections()[i].output == input) {
                    return true;
                }

                if ( IsCyclic(current.output.GetConnections()[i], input) ) {
                    return true;
                }

                current = current.output.GetConnections()[i];
            }
        }

        return false;
    }
}
