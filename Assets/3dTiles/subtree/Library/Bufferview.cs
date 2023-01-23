namespace subtree
{

    public record Bufferview
    {
        public int buffer { get; set; }
        public int byteOffset { get; set; }
        public int byteLength { get; set; }
    }
}
