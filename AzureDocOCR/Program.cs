using System.Diagnostics;
using Azure;
using Azure.AI.DocumentIntelligence;

namespace InvoiceRecognitionSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string endpoint = "";
            string key = "";

            var credential = new AzureKeyCredential(key);
            var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

            //var image = BinaryData.FromBytes(File.ReadAllBytes("path/to/invoice.png"));
            //var request = new AnalyzeDocumentOptions("prebuilt-invoice", image);

            // https://user-images.githubusercontent.com/19628355/251216717-7e465227-cad3-4a85-b505-9c4441b31967.png
            //https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/invoice_sample.jpg
            //https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/media/contoso-invoice.png
            string formUrl = "https://user-images.githubusercontent.com/19628355/251216717-7e465227-cad3-4a85-b505-9c4441b31967.png";

            var timer = Stopwatch.StartNew();

            var request = new AnalyzeDocumentOptions("prebuilt-invoice", new Uri(formUrl));
            var result = await client.AnalyzeDocumentAsync(WaitUntil.Completed, request);

            foreach (var invoice in result.Value.Documents)
            {
                // Print header fields.
                PrintField(invoice.Fields, "VendorName", "Vendor Name", f => f.ValueString);
                PrintField(invoice.Fields, "VendorAddress", "Vendor Address", f => f.Content);
                PrintField(invoice.Fields, "VendorAddressRecipient", "Vendor Address Recipient", f => f.ValueString);
                PrintField(invoice.Fields, "CustomerName", "Customer Name", f => f.ValueString);
                PrintField(invoice.Fields, "CustomerId", "Customer Id", f => f.ValueString);
                // Customer Address uses a slightly different format.
                PrintField(invoice.Fields, "CustomerAddress", "Customer Address", f => $"\n{{{f.Content}}}");
                PrintField(invoice.Fields, "CustomerAddressRecipient", "Customer Address Recipient", f => f.ValueString);
                PrintField(invoice.Fields, "InvoiceId", "Invoice Id", f => f.ValueString);
                PrintField(invoice.Fields, "InvoiceDate", "Invoice Date", f => f.ValueDate.ToString());
                PrintField(invoice.Fields, "InvoiceTotal", "Invoice Total", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoice.Fields, "DueDate", "Due Date", f => f.ValueDate.ToString());
                PrintField(invoice.Fields, "PurchaseOrder", "Purchase Order", f => f.ValueString);
                PrintField(invoice.Fields, "BillingAddress", "Billing Address", f => f.Content);
                PrintField(invoice.Fields, "BillingAddressRecipient", "Billing Address Recipient", f => f.ValueString);
                PrintField(invoice.Fields, "ShippingAddress", "Shipping Address", f => $"\n{{{f.Content}}}");
                PrintField(invoice.Fields, "ShippingAddressRecipient", "Shipping Address Recipient", f => f.ValueString);

                // Process invoice items.
                if (invoice.Fields.TryGetValue("Items", out DocumentField itemsField))
                {
                    Console.WriteLine("Invoice items:");
                    foreach (var itemField in itemsField.ValueList)
                    {
                        Console.WriteLine("...Item");
                        var item = itemField.ValueDictionary;
                        PrintField(item, "Description", "......Description", f => f.ValueString);
                        PrintField(item, "Quantity", "......Quantity", f => f.ValueDouble.ToString());
                        PrintField(item, "UnitPrice", "......Unit Price", f => f.ValueCurrency.Amount.ToString());
                        PrintField(item, "Amount", "......Amount", f => f.ValueCurrency.Amount.ToString());
                    }
                }

                // Print summary fields.
                PrintField(invoice.Fields, "SubTotal", "Subtotal", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoice.Fields, "TotalTax", "Total Tax", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoice.Fields, "PreviousUnpaidBalance", "Previous Unpaid Balance", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoice.Fields, "AmountDue", "Amount Due", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoice.Fields, "ServiceStartDate", "Service Start Date", f => f.ValueDate.ToString());
                PrintField(invoice.Fields, "ServiceEndDate", "Service End Date", f => f.ValueDate.ToString());
                PrintField(invoice.Fields, "ServiceAddress", "Service Address", f => $"\n{{{f.Content}}}");
                PrintField(invoice.Fields, "ServiceAddressRecipient", "Service Address Recipient", f => f.ValueString);
                PrintField(invoice.Fields, "RemittanceAddress", "Remittance Address", f => f.Content);
                PrintField(invoice.Fields, "RemittanceAddressRecipient", "Remittance Address Recipient", f => f.ValueString);
            }

            timer.Stop();
            Console.WriteLine($"Elapsed time: {timer.Elapsed}");
        }

        private static void PrintField(
            DocumentFieldDictionary fields,
            string fieldName,
            string label,
            Func<DocumentField, string> valueSelector)
        {
            if (fields.TryGetValue(fieldName, out var field))
            {
                Console.WriteLine($"{label}: {valueSelector(field)} has confidence: {field.Confidence}");
            }
        }
    }
}
