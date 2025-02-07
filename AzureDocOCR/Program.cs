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

            Dictionary<int, string> imageMap = new Dictionary<int, string>
            {
                { 1, "https://user-images.githubusercontent.com/19628355/251216717-7e465227-cad3-4a85-b505-9c4441b31967.png" },
                { 2, "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/invoice_sample.jpg" },
                { 3, "https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/media/contoso-invoice.png" },
            };


            var timer = Stopwatch.StartNew();

            var request = new AnalyzeDocumentOptions("prebuilt-invoice", new Uri(imageMap[1]));

            var result = await client.AnalyzeDocumentAsync(WaitUntil.Completed, request);

            foreach (var invoiceFields in result.Value.Documents.Select(invoice => invoice.Fields))
            {
                // Print header fields.
                PrintField(invoiceFields, "VendorName", "Vendor Name", f => f.ValueString);
                PrintField(invoiceFields, "VendorAddress", "Vendor Address", f => $"\n{{{f.Content}}}");
                PrintField(invoiceFields, "VendorAddressRecipient", "Vendor Address Recipient", f => f.ValueString);
                PrintField(invoiceFields, "CustomerName", "Customer Name", f => f.ValueString);
                PrintField(invoiceFields, "CustomerId", "Customer Id", f => f.ValueString);
                // Customer Address uses a slightly different format.
                PrintField(invoiceFields, "CustomerAddress", "Customer Address", f => $"\n{{{f.Content}}}");
                PrintField(invoiceFields, "CustomerAddressRecipient", "Customer Address Recipient", f => f.ValueString);
                PrintField(invoiceFields, "InvoiceId", "Invoice Id", f => f.ValueString);
                PrintField(invoiceFields, "InvoiceDate", "Invoice Date", f => f.ValueDate.ToString());
                PrintField(invoiceFields, "InvoiceTotal", "Invoice Total", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoiceFields, "DueDate", "Due Date", f => f.ValueDate.ToString());
                PrintField(invoiceFields, "PurchaseOrder", "Purchase Order", f => f.ValueString);
                PrintField(invoiceFields, "BillingAddress", "Billing Address", f => f.Content);
                PrintField(invoiceFields, "BillingAddressRecipient", "Billing Address Recipient", f => f.ValueString);
                PrintField(invoiceFields, "ShippingAddress", "Shipping Address", f => $"\n{{{f.Content}}}");
                PrintField(invoiceFields, "ShippingAddressRecipient", "Shipping Address Recipient", f => f.ValueString);

                // Process invoice items.
                if (invoiceFields.TryGetValue("Items", out DocumentField itemsField))
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
                PrintField(invoiceFields, "SubTotal", "Subtotal", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoiceFields, "TotalTax", "Total Tax", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoiceFields, "PreviousUnpaidBalance", "Previous Unpaid Balance", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoiceFields, "AmountDue", "Amount Due", f => f.ValueCurrency.Amount.ToString());
                PrintField(invoiceFields, "ServiceStartDate", "Service Start Date", f => f.ValueDate.ToString());
                PrintField(invoiceFields, "ServiceEndDate", "Service End Date", f => f.ValueDate.ToString());
                PrintField(invoiceFields, "ServiceAddress", "Service Address", f => $"\n{{{f.Content}}}");
                PrintField(invoiceFields, "ServiceAddressRecipient", "Service Address Recipient", f => f.ValueString);
                PrintField(invoiceFields, "RemittanceAddress", "Remittance Address", f => f.Content);
                PrintField(invoiceFields, "RemittanceAddressRecipient", "Remittance Address Recipient", f => f.ValueString);
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
