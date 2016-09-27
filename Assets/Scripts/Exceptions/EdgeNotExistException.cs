using System;

public class EdgeNotExistException : Exception {
    public override string Message
    {
        get { return "Expected edge isn't exist"; }
    }
}
