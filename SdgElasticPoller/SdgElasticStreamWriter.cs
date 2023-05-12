using System.Text.Json;

namespace SdgElasticPoller
{
    public static class SdgElasticStreamWriter
    {
        public static async Task WriteBulkSdgIndexRequestAsync(this Utf8JsonWriter jsonWriter, Action writeNewLine, JsonElement element, string elasticIndex, CancellationToken token)
        {
            if (!element.TryGetProperty("uuid", out var uuidProp))
            {
                return;
            }


            var id = $"kennisartikel_{uuidProp.GetString()}";

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("index");
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("_index", elasticIndex);
            jsonWriter.WriteString("_id", id);

            jsonWriter.WriteEndObject();
            await jsonWriter.FlushAsync(token);
            jsonWriter.Reset();
            writeNewLine();
            jsonWriter.WriteSdgIndexRequest(element, id);
            await jsonWriter.FlushAsync(token);
            jsonWriter.Reset();
            writeNewLine();
        }

        private static void WriteSdgIndexRequest(this Utf8JsonWriter jsonWriter, JsonElement sdgProduct, string id)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("id", id);

            if (sdgProduct.TryGetProperty("vertalingen", out var vertalingenProp) && vertalingenProp.ValueKind == JsonValueKind.Array)
            {
                var vertaling = vertalingenProp[0];
                if (vertaling.TryGetProperty("titel", out var titelProp) && titelProp.ValueKind == JsonValueKind.String)
                {
                    jsonWriter.WritePropertyName("title");
                    titelProp.WriteTo(jsonWriter);
                }
                if (vertaling.TryGetProperty("tekst", out var tekstProp) && tekstProp.ValueKind == JsonValueKind.String)
                {
                    jsonWriter.WritePropertyName("object_meta");
                    tekstProp.WriteTo(jsonWriter);
                }
            }

            jsonWriter.WriteString("object_bron", "Kennisartikel");
            jsonWriter.WritePropertyName("object");

            sdgProduct.WriteTo(jsonWriter);

            jsonWriter.WriteEndObject();
        }
    }
}
