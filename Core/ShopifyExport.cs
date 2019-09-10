using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyScraper
{
    public class ShopifyExport : AbstractCsvExport
    {
        public override string[] Headers => new string[]
            { "Handle", "Title", "Body (HTML)", "Vendor", "Type", "Tags", "Published", "Option1 Name", "Option1 Value", "Option2 Name", "Option2 Value", "Option3 Name", "Option3 Value", "Variant SKU", "Variant Grams", "Variant Inventory Tracker", "Variant Inventory Qty", "Variant Inventory Policy", "Variant Fulfillment Service", "Variant Price", "Variant Compare At Price", "Variant Requires Shipping", "Variant Taxable", "Variant Barcode", "Image Src", "Image Position", "Image Alt Text", "Gift Card", "Google Shopping / MPN", "Google Shopping / Age Group", "Google Shopping / Gender", "Google Shopping / Google Product Category", "SEO Title", "SEO Description", "Google Shopping / AdWords Grouping", "Google Shopping / AdWords Labels", "Google Shopping / Condition", "Google Shopping / Custom Product", "Google Shopping / Custom Label 0", "Google Shopping / Custom Label 1", "Google Shopping / Custom Label 2", "Google Shopping / Custom Label 3", "Google Shopping / Custom Label 4", "Variant Image", "Variant Weight Unit" };

        public override string Name => "Shopify";

        public override List<string[]> ParseProduct(Product product)
        {
            if (product != null)
            {
                var list = new List<string[]>();
                try
                {
                    string handle = product.handle;
                    string title = product.title;
                    string body = product.body_html;
                    string vendor = product.vendor;
                    string published = product.published_at;
                    string op1Name = product.options.Length > 0 ? product.options[0].name : "";
                    string op2Name = product.options.Length > 1 ? product.options[1].name : "";
                    string op3Name = product.options.Length > 2 ? product.options[2].name : "";

                    int imageLength = product.images.Length;

                    bool first = true;
                    foreach (Variant item in product.variants)
                    {
                        var rowtemp = new string[] {
                            handle, // Handle
                            first?title:string.Empty, // Title
                            first?body:string.Empty, // Body (HTML)
                            first?vendor:string.Empty, // Vendor
                            "", // Type
                            "", // tag
                            first?"TRUE":string.Empty, // Published
                            first?op1Name:string.Empty, // Option1 Name
                            item.option1, // Option1 Value
                            first?op2Name:string.Empty, // Option2 Name
                            item.option2, // Option2 Value
                            first?op3Name:string.Empty, // Option3 Name
                            item.option3, // Option3 Value
                            item.sku, // Variant SKU
                            "0", // Variant Grams
                            "", // Variant Inventory Tracker
                            "1", // Variant Inventory Qty
                            "deny", // Variant Inventory Policy
                            "manual", // Variant Fulfillment Service
                            item.price,
                            item.compare_at_price,
                            "TRUE", // Variant Requires Shipping
                            "TRUE", // Variant Taxable
                            "", // Variant Barcode
                            item.position < imageLength? product.images[item.position].src: string.Empty, // Image Src
                            item.position < imageLength? item.position.ToString(): string.Empty, // Image Position
                            "", // Image Alt Text
                            first?"FALSE":string.Empty, // Gift Card
                            "", // Google Shopping / MPN
                            "", // Google Shopping / Age Group
                            "", // Google Shopping / Gender
                            "", // Google Shopping / Google Product Category
                            "", // SEO Title
                            "", // SEO Description
                            "", // Google Shopping / AdWords Grouping
                            "", // Google Shopping / AdWords Labels
                            "", // Google Shopping / Condition
                            "", // Google Shopping / Custom Product
                            "", // Google Shopping / Custom Label 0
                            "", // Google Shopping / Custom Label 1
                            "", // Google Shopping / Custom Label 2
                            "", // Google Shopping / Custom Label 3
                            "", // Google Shopping / Custom Label 4
                            item.featured_image.src, // Variant Image
                            "kg", // Variant Weight Unit

                        };
                        list.Add(rowtemp);
                        first = false;
                    }
                }
                catch (Exception)
                {
                    // silence
                }
                return list;
            }
            return null;
        }
    }
}
