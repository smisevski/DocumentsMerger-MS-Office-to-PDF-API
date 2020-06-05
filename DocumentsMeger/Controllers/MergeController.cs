﻿using DocumentsMerger.Models;
using System;
using System.Web.Http;
using iText;


namespace DocumentsMerger.Controllers
{
    public class MergeController : ApiController
    {

        public iText.Kernel.Pdf.PdfDocument resultPdf;

        public const string resultPath = "C:\\ResultPrints\\";


        // POST api/merge  => params from SugarCRM { unique_filepath, filepath, filenames[] }
        [Route("api/merge"), HttpPost]
        public IHttpActionResult MergeDocs(DocumentsData documentsData)
        {

            System.IO.Directory.CreateDirectory(resultPath + documentsData.unique_filepath);

            this.resultPdf = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfWriter(resultPath + documentsData.unique_filepath + "\\resultMerge.pdf"));
            this.resultPdf.Close();

            foreach (string filename in documentsData.filenames)
            {

                string[] filename_parts = filename.Split('.');

                string sourceFilePath = documentsData.filepath + filename;

                if (filename_parts[1] == "docx")
                {

                    Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();

                    Microsoft.Office.Interop.Word.Document doc = app.Documents.Open(sourceFilePath, true, true, false);

                    doc.Activate();

                    string printPath = resultPath + documentsData.unique_filepath + "\\" + filename_parts[0] + ".pdf";

                    doc.ExportAsFixedFormat(printPath.ToString(), Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);

                    doc.Close();

                    this.GenerateSinglePdf(printPath);

                }
                else if (filename_parts[1] == "xlsx")
                {

                    Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();

                    Microsoft.Office.Interop.Excel.Workbook wb = app.Workbooks.Open(sourceFilePath);

                    wb.Activate();

                    string printPath = resultPath + documentsData.unique_filepath + "\\" + filename_parts[0] + ".pdf";

                    wb.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, printPath.ToString());

                    wb.Close();

                    this.GenerateSinglePdf(printPath);

                }
                else
                {
                    throw new Exception("Wrong file format. Only docx/xlsx 2007-2013/16 file formats are supported.");
                }

            }

            this.resultPdf.Close();

            return Ok(this.resultPdf);
        }


        private void GenerateSinglePdf(string targetPDF)
        {

            iText.Kernel.Pdf.PdfDocument docToMerge = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(targetPDF));

            docToMerge.CopyPagesTo(1, docToMerge.GetNumberOfPages(), this.resultPdf);

        }

    }
}
