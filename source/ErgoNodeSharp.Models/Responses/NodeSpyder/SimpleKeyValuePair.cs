namespace ErgoNodeSharp.Models.Responses.NodeSpyder
{
    public class SimpleKeyValuePair<T>
    {
        public T Key { get; set; }

        public int Value { get; set; }
    }

    public class StringValuePair : SimpleKeyValuePair<string>
    {

    }

    public class BoolValuePair : SimpleKeyValuePair<bool>
    {

    }

    public class IntValuePair : SimpleKeyValuePair<int>
    {

    }
}
