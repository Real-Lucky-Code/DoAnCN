using Microsoft.ML.Data;

namespace WebBanHang.AI
{

    public class ProductData
    {
        [LoadColumn(0)]
        public string Name { get; set; } = string.Empty;
    }

    public class ProductPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;
    }
}