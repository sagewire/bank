namespace bank.poco
{
    public class ChartRowValueConfig
    {
        public string Text { get; set; }
        public bool IsFormula { get; set; } = false;
        public ChartColumnConfig ColumnConfig { get; set; }
    }
}