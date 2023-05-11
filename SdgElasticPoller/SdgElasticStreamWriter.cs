using System.Text.Json;

namespace SdgElasticPoller
{
    public static class SdgElasticStreamWriter
    {
        const byte NewLine = (byte)'\n';

        public static void WriteBulkWriteSdgIndexRequest(this Stream outputStream, JsonElement element, string elasticIndex)
        {
            using var jsonWriter = new Utf8JsonWriter(outputStream);
            jsonWriter.WriteSdgIndexRequest(elasticIndex, element);
            outputStream.WriteByte(NewLine);
        }

        private static void WriteSdgIndexRequest(this Utf8JsonWriter jsonWriter, string elasticIndex, JsonElement sdgProduct)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("index");
            jsonWriter.WriteStartObject();

            jsonWriter.WriteString("_index", elasticIndex);

            if (sdgProduct.TryGetProperty("uuid", out var uuidProp))
            {
                var uuid = uuidProp.GetString();
                jsonWriter.WriteString("id", $"kennisartikel_{uuid}");
                jsonWriter.WriteString("_id", $"kennisartikel_{uuid}");
            }

            if (sdgProduct.TryGetProperty("vertalingen", out var vertalingenProp) && vertalingenProp.ValueKind == JsonValueKind.Array)
            {
                var vertaling = vertalingenProp[0];
                if (vertaling.TryGetProperty("titel", out var titelProp) && titelProp.ValueKind == JsonValueKind.String)
                {
                    jsonWriter.WriteString("title", titelProp.GetString());
                }
                if (vertaling.TryGetProperty("tekst", out var tekstProp) && tekstProp.ValueKind == JsonValueKind.String)
                {
                    jsonWriter.WriteString("object_meta", tekstProp.GetString());
                }
            }

            jsonWriter.WriteString("object_bron", "Kennisartikel");
            jsonWriter.WritePropertyName("object");

            sdgProduct.WriteTo(jsonWriter);

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }
    }
}
