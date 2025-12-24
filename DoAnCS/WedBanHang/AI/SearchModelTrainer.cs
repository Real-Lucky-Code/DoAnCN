using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebBanHang.AI
{
    public class SearchModelTrainer
    {
        private static string _modelPath = Path.Combine("AI", "Models", "searchModel.zip");
        private static MLContext _mlContext = new MLContext();
        private static ITransformer _trainedModel;
        private static PredictionEngine<ProductInput, ProductPrediction> _predictionEngine;

        public class ProductInput
        {
            [LoadColumn(0)] public string Name { get; set; }
            [LoadColumn(1)] public string Description { get; set; }
            [LoadColumn(2)] public string CategoryName { get; set; }
        }

        public class ProductPrediction
        {
            [ColumnName("PredictedLabel")]
            public string Prediction { get; set; }
        }

        /// Huấn luyện mô hình từ danh sách sản phẩm
        public static void TrainModel(IEnumerable<ProductInput> products)
        {
            var trainingData = _mlContext.Data.LoadFromEnumerable(products);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("NameFeats", nameof(ProductInput.Name))
                .Append(_mlContext.Transforms.Text.FeaturizeText("DescFeats", nameof(ProductInput.Description)))
                .Append(_mlContext.Transforms.Text.FeaturizeText("CatFeats", nameof(ProductInput.CategoryName)))
                .Append(_mlContext.Transforms.Concatenate("Features", "NameFeats", "DescFeats", "CatFeats"))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ProductInput.Name)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _trainedModel = pipeline.Fit(trainingData);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductInput, ProductPrediction>(_trainedModel);

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath));
            _mlContext.Model.Save(_trainedModel, trainingData.Schema, _modelPath);
        }

        public static void LoadModel()
        {
            if (File.Exists(_modelPath))
            {
                using var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _trainedModel = _mlContext.Model.Load(stream, out _);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductInput, ProductPrediction>(_trainedModel);
            }
        }

        public static string Predict(ProductInput input)
        {
            try
            {
                if (_predictionEngine == null)
                    LoadModel();

                if (_predictionEngine == null)
                    return ""; // model chưa được load

                var result = _predictionEngine.Predict(input);
                return result.Prediction ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi dự đoán: {ex.Message}");
                return "";
            }
        }

    }
}