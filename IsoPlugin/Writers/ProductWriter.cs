﻿using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class ProductWriter : BaseWriter
    {
        private ProductWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "PDT")
        {
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.FertilizerProducts == null ||
                taskWriter.DataModel.Catalog.ProductMixes == null)
                return;

            var writer = new ProductWriter(taskWriter);
            writer.WriteProducts();
        }

        private void WriteProducts()
        {
            WriteToExternalFile(WriteProducts);
        }

        private void WriteProducts(XmlWriter writer)
        {
            WriteProducts(writer, TaskWriter.DataModel.Catalog.FertilizerProducts);
            WriteProductMixes(writer, TaskWriter.DataModel.Catalog.ProductMixes);
        }

        private void WriteProducts(XmlWriter writer, List<FertilizerProduct> products)
        {
            foreach (var product in products)
            {
                var productId = WriteProduct(writer, product);
                TaskWriter.Products[product.Id.ReferenceId] = productId;
            }
        }

        private void WriteProductMixes(XmlWriter writer, List<ProductMix> productMixes)
        {
            foreach (var productMix in productMixes)
            {
                var productId = WriteProductMix(writer, productMix);
                TaskWriter.Products[productMix.Id.ReferenceId] = productId;
            }
        }

        private string WriteProduct(XmlWriter writer, FertilizerProduct product)
        {
            var productId = GenerateId();
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", product.Description);

            writer.WriteEndElement();

            return productId;
        }

        private string WriteProductMix(XmlWriter writer, ProductMix productMix)
        {
            var productId = GenerateId();
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", productMix.Description);
            writer.WriteAttributeString("F", "2");

            WriteTotalQuantity(writer, productMix.TotalQuantity);
            WriteProductComponents(writer, productMix.ProductComponents);

            writer.WriteEndElement();

            return productId;
        }

        private static void WriteTotalQuantity(XmlWriter writer, NumericRepresentationValue quantity)
        {
            if (quantity == null || quantity.Value == null)
                return;

            writer.WriteXmlAttribute("G", quantity.Value.Value.ToString("F0", CultureInfo.InvariantCulture));
        }

        private void WriteProductComponents(XmlWriter writer, List<ProductComponent> productComponents)
        {
            foreach (var productComponent in productComponents)
            {
                WriteProductComponent(writer, productComponent);
            }
        }

        private void WriteProductComponent(XmlWriter writer, ProductComponent productComponent)
        {
            var productId = TaskWriter.Products.FindById(productComponent.IngredientId);
            if (string.IsNullOrEmpty(productId) ||
                productComponent.Quantity == null ||
                productComponent.Quantity.Value == null)
                return;

            writer.WriteStartElement("PLN");
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", productComponent.Quantity.Value.Value.ToString("F0", CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }
}