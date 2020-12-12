using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace budoco.Pages
{
    public sealed class ViewReportModel : PageModel
    {
        private const int DefaultScale = 1;

        [FromQuery]
        public int id { get; set; }

        [FromQuery]
        public string view { get; set; }

        [FromQuery]
        public int scale { get; set; } = DefaultScale;

        public DataTable dt;

        public IActionResult OnGet()
        {

            var sql = @"
                select
                    rp_name,
                    rp_sql,
                    rp_chart_type
                from
                    reports
                where
                    rp_id = @id";

            var dict = new Dictionary<string, object>
            {
                ["@id"] = id
            };

            var dr = bd_db.get_datarow(sql, dict);

            var rp_sql = (string)dr["rp_sql"];
            var chart_type = (string)dr["rp_chart_type"];
            var desc = (string)dr["rp_name"];

            // replace the magic pseudo variable
            rp_sql = rp_sql.Replace("$ME", HttpContext.Session.GetInt32("us_id").ToString());

            dt = bd_db.get_datatable(rp_sql);

            var needDrawGraph = chart_type == "pie"
                || chart_type == "bar"
                || chart_type == "line";

            if (view == "data" || !needDrawGraph)
            {
                ViewData["Title"] = desc;

                return Page();
            }

            if (dt.Rows.Count > 0)
            {
                if (chart_type == "pie")
                {
                    using var bitmap = CreatePieChart(desc, dt);
                    using var memoryStream = new MemoryStream();

                    // Save the image to a stream
                    bitmap.Save(memoryStream, ImageFormat.Gif);

                    return File(memoryStream.ToArray(), MediaTypeNames.Image.Gif);
                }

                if (chart_type == "bar")
                {
                    using var bitmap = CreateBarChart(desc, dt, scale);
                    using var memoryStream = new MemoryStream();

                    // Save the image to a stream
                    bitmap.Save(memoryStream, ImageFormat.Gif);

                    return File(memoryStream.ToArray(), MediaTypeNames.Image.Gif);
                }

                if (chart_type == "line")
                {
                    // we need at least two values to draw a line
                    if (dt.Rows.Count > 1)
                    {
                        using var bitmap = CreateLineChart(desc, dt, scale);
                        using var memoryStream = new MemoryStream();

                        // Save the image to a stream
                        bitmap.Save(memoryStream, ImageFormat.Gif);

                        return File(memoryStream.ToArray(), MediaTypeNames.Image.Gif);
                    }
                    else
                    {
                        using var bitmap = WriteNoDataMessage(desc, scale);
                        using var memoryStream = new MemoryStream();

                        // Save the image to a stream
                        bitmap.Save(memoryStream, ImageFormat.Gif);

                        return File(memoryStream.ToArray(), MediaTypeNames.Image.Gif);
                    }
                }

                return Content(string.Empty);
            }
            else
            {
                using var bitmap = WriteNoDataMessage(desc, scale);
                using var memoryStream = new MemoryStream();

                // Save the image to a stream
                bitmap.Save(memoryStream, ImageFormat.Gif);

                return File(memoryStream.ToArray(), MediaTypeNames.Image.Gif);
            }
        }

        public IEnumerable<string> Columns()
        {
            return dt.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName);
        }

        public IEnumerable<string> Values(DataRow row)
        {
            return row.ItemArray
                .Select(x => x.ToString());
        }

        private static Bitmap CreatePieChart(string title, DataTable dataTable)
        {
            var width = 240;
            var pageTopMargin = 15;

            // [corey] - I downloaded this code from MSDN, the URL below, and modified it.
            // http://msdn.microsoft.com/msdnmag/issues/02/02/ASPDraw/default.aspx

            // We need to connect to the database and grab information for the
            // particular columns for the particular table

            // find the total of the numeric data
            float total = 0.0F, tmp;
            int i;

            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                tmp = Convert.ToSingle(dataTable.Rows[i][1]);
                total += tmp;
            }

            // we need to create fonts for our legend and title
            var fontLegend = new Font("Arial", 10);

            var fontTitle = new Font("Arial", 12, FontStyle.Bold);
            var titleHeight = fontTitle.Height + pageTopMargin;

            // We need to create a legend and title, how big do these need to be?
            // Also, we need to resize the height for the pie chart, respective to the
            // height of the legend and title

            var rowGap = 6;
            var startOfRect = 8;
            var rectWidth = 14;
            var rectHeight = 16;

            int rowHeight;
            if (rectHeight > fontLegend.Height) rowHeight = rectHeight;
            else rowHeight = fontLegend.Height;
            rowHeight += rowGap;

            var legendHeight = rowHeight * (dataTable.Rows.Count + 1);
            var height = width + legendHeight + titleHeight + pageTopMargin;
            var pieHeight = width; // maintain a one-to-one ratio

            // Corey says... quick and dirty fix porting this from BugTracker.NET
            // to Budoco. Instead of images on white background, Chrome displays on
            // black background, so we need the margin otherwise image looks bad.
            var extra_x = 50;

            // Create a rectange for drawing our pie
            var pieRect = new Rectangle(0 + (extra_x / 2), titleHeight, width, pieHeight);

            // Create our pie chart, start by creating an ArrayList of colors
            var colors = new ArrayList
            {
                new SolidBrush(Color.FromArgb(204, 204, 255)),
                new SolidBrush(Color.FromArgb(051, 051, 255)),
                new SolidBrush(Color.FromArgb(204, 204, 204)),
                new SolidBrush(Color.FromArgb(153, 153, 255)),
                new SolidBrush(Color.FromArgb(153, 153, 153)),
                new SolidBrush(Color.FromArgb(000, 204, 000))
            };

            var rnd = new Random();

            for (i = 0; i < dataTable.Rows.Count - 6; i++)
            {
                colors.Add(new SolidBrush(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))));
            }

            var currentDegree = 0.0F;

            // Create a Bitmap instance
            var objBitmap = new Bitmap(width + extra_x, height + extra_x);

            using var objGraphics = Graphics.FromImage(objBitmap);
            using var blackBrush = new SolidBrush(Color.Black);
            using var whiteBrush = new SolidBrush(Color.White);

            // Put a white backround in
            objGraphics.FillRectangle(whiteBrush, 0, 0, width + extra_x, height + (extra_x / 2));
            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                objGraphics.FillPie(
                    (SolidBrush)colors[i],
                    pieRect,
                    currentDegree,
                    Convert.ToSingle(dataTable.Rows[i][1]) / total * 360);

                // increment the currentDegree
                currentDegree += Convert.ToSingle(dataTable.Rows[i][1]) / total * 360;
            }

            // Create the title, centered
            using var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            objGraphics.DrawString(title, fontTitle, blackBrush, new Rectangle(0, 0, width, titleHeight), stringFormat);

            // Create the legend
            objGraphics.DrawRectangle(
                new Pen(Color.Gray, 1),
                0 + (extra_x / 2),
                height - legendHeight,
                width - 4,
                legendHeight - 1);

            var y = height - legendHeight + rowGap;

            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                objGraphics.FillRectangle(
                    (SolidBrush)colors[i],
                    startOfRect, // x
                    y,
                    rectWidth,
                    rectHeight);

                objGraphics.DrawString(
                    Convert.ToString(dataTable.Rows[i][0])
                    + " - " +
                    Convert.ToString(dataTable.Rows[i][1]),
                    fontLegend,
                    blackBrush,
                    startOfRect + rectWidth + 4,
                    y);

                y += rectHeight + rowGap;
            }

            // display the total
            objGraphics.DrawString(
                $"Total: {total}",
                fontLegend,
                blackBrush,
                startOfRect + rectWidth + 4,
                y);

            return objBitmap;
        }

        private static Bitmap CreateBarChart(string title, DataTable dataTable, int scale)
        {
            var chartWidth = 640 / scale;
            var chartHeight = 300 / scale;
            var chartTopMargin = 10 / scale; // gap between highest bar and border of chart

            var xAxisTextOffset = 8 / scale; // gap between edge and start of x axis text
            var pageTopMargin = 40 / scale; // gape between chart and top of page

            var maxGridLines = 20 / scale;

            var fontTitle = new Font("Arial", 12, FontStyle.Bold);

            var fontLegend = new Font("Arial", 8);
            var pageBottomMargin = 3 * fontLegend.Height;
            var pageLeftMargin = 4 * fontLegend.Height + xAxisTextOffset; // where the y axis text goes

            // find the max of the y axis so we know how to scale the data
            var max = 0.0F;
            float tmp;
            int i;

            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                tmp = Convert.ToSingle(dataTable.Rows[i][1]);
                if (tmp > max) max = tmp;
            }

            var verticalScaleFactor = (chartHeight - chartTopMargin) / max;

            // determine how the horizontal grid lines should be

            var gridLineInterval = 1;

            if (max > 1)
                while (max / gridLineInterval > maxGridLines)
                    gridLineInterval *= 10 / scale;

            // Corey's quick and dirty fix for Budoco
            int extra_x = 50;

            // Create a Bitmap instance
            var objBitmap = new Bitmap(
                pageLeftMargin + chartWidth + extra_x, // total width
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // total height

            using var objGraphics = Graphics.FromImage(objBitmap);
            using var blackBrush = new SolidBrush(Color.Black);
            using var whiteBrush = new SolidBrush(Color.White);

            // white overall background
            objGraphics.FillRectangle(
                whiteBrush, // yellow
                0, 0,
                pageLeftMargin + chartWidth + extra_x, // far left
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // bottom

            // gray chart background
            objGraphics.FillRectangle(
                new SolidBrush(Color.FromArgb(204, 204, 204)), // gray
                pageLeftMargin, pageTopMargin + fontTitle.Height,
                pageLeftMargin + chartWidth - extra_x,
                chartHeight);

            // draw title
            objGraphics.DrawString(
                title,
                fontTitle,
                blackBrush,
                xAxisTextOffset,
                fontTitle.Height / 2);

            int y;
            var chartBottom = pageTopMargin + fontTitle.Height + chartHeight;

            var blackPen = new Pen(Color.Black, 1);

            for (i = 0; i < max; i += gridLineInterval)
            {
                y = (int)(i * verticalScaleFactor);

                // y axis label
                objGraphics.DrawString(
                    Convert.ToString(i),
                    fontLegend,
                    blackBrush,
                    xAxisTextOffset, chartBottom - y - fontLegend.Height / 2);

                // grid line
                objGraphics.DrawLine(
                    blackPen,
                    pageLeftMargin,
                    chartBottom - y,
                    pageLeftMargin + chartWidth,
                    chartBottom - y);
            }


            // draw bars
            var barSpace = chartWidth / dataTable.Rows.Count;
            var barWidth = (int)(.70F * barSpace);
            var x = (int)(.30F * barSpace);
            x += pageLeftMargin;

            var xAxisTextY = chartBottom + pageBottomMargin / 2 - fontLegend.Height / 2;
            Brush blueBrush = new SolidBrush(Color.FromArgb(0, 0, 204));

            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                var data = Convert.ToSingle(dataTable.Rows[i][1]);

                var barHeight = (int)(data * verticalScaleFactor);

                objGraphics.FillRectangle(
                    blueBrush,
                    x, chartBottom - barHeight,
                    barWidth,
                    barHeight);

                objGraphics.DrawString(
                    Convert.ToString(dataTable.Rows[i][0]),
                    fontLegend,
                    blackBrush,
                    x, xAxisTextY);

                x += barWidth;
                x += (int)(.30F * barSpace);
            }

            return objBitmap;
        }

        private static Bitmap CreateLineChart(string title, DataTable dataTable, int scale)
        {
            var chartWidth = 640 / scale;
            var chartHeight = 300 / scale;
            var chartTopMargin = 10 / scale; // gap between highest bar and border of chart

            var xAxisTextOffset = 8 / scale; // gap between edge and start of x axis text
            var pageTopMargin = 40 / scale; // gape between chart and top of page

            var maxGridLines = 20 / scale;

            var fontTitle = new Font("Arial", 12, FontStyle.Bold);

            var fontLegend = new Font("Arial", 8);
            var pageBottomMargin = 3 * fontLegend.Height;
            var pageLeftMargin = 4 * fontLegend.Height + xAxisTextOffset; // where the y axis text goes

            // find the max of the y axis so we know how to scale the data
            var max = 0.0F;
            float tmp;
            int i;

            for (i = 0; i < dataTable.Rows.Count; i++)
            {
                tmp = Convert.ToSingle(dataTable.Rows[i][1]);
                if (tmp > max) max = tmp;
            }

            var verticalScaleFactor = (chartHeight - chartTopMargin) / max;

            // determine how the horizontal grid lines should be

            var gridLineInterval = 1;
            if (max > 1)
                while (max / gridLineInterval > maxGridLines)
                    gridLineInterval *= 10 / scale;

            // Corey's quick and dirty fix for Budoco
            int extra_x = 50;

            // Create a Bitmap instance
            var objBitmap = new Bitmap(
                pageLeftMargin + chartWidth + extra_x, // total width
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // total height

            using var objGraphics = Graphics.FromImage(objBitmap);
            using var blackBrush = new SolidBrush(Color.Black);
            using var whiteBrush = new SolidBrush(Color.White);

            // white overall background
            objGraphics.FillRectangle(
                whiteBrush,
                0, 0,
                pageLeftMargin + chartWidth + extra_x, // far left
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // bottom

            // gray chart background
            objGraphics.FillRectangle(
                new SolidBrush(Color.FromArgb(204, 204, 204)), // gray
                pageLeftMargin, pageTopMargin + fontTitle.Height,
                pageLeftMargin + chartWidth - extra_x,
                chartHeight);

            // draw title
            objGraphics.DrawString(
                title,
                fontTitle,
                blackBrush,
                xAxisTextOffset,
                fontTitle.Height / 2);

            int y;
            var chartBottom = pageTopMargin + fontTitle.Height + chartHeight;

            var blackPen = new Pen(Color.Black, 1);

            for (i = 0; i < max; i += gridLineInterval)
            {
                y = (int)(i * verticalScaleFactor);

                // y axis label
                objGraphics.DrawString(
                    Convert.ToString(i),
                    fontLegend,
                    blackBrush,
                    xAxisTextOffset, chartBottom - y - fontLegend.Height / 2);

                // grid line
                objGraphics.DrawLine(
                    blackPen,
                    pageLeftMargin,
                    chartBottom - y,
                    pageLeftMargin + chartWidth,
                    chartBottom - y);
            }

            // draw lines
            var lineLength = chartWidth / (dataTable.Rows.Count - 1);
            var x = pageLeftMargin;

            var xAxisTextY = chartBottom + pageBottomMargin / 2 - fontLegend.Height / 2;

            var bluePen = new Pen(Color.FromArgb(0, 0, 204), 2);
            var blueBrush = new SolidBrush(Color.FromArgb(0, 0, 204));
            var prevXAxisLabel = -99999;

            for (i = 1; i < dataTable.Rows.Count; i++)
            {
                var data1 = Convert.ToSingle(dataTable.Rows[i - 1][1]);
                var data2 = Convert.ToSingle(dataTable.Rows[i][1]);

                var valueY1 = (int)(data1 * verticalScaleFactor);
                var valueY2 = (int)(data2 * verticalScaleFactor);

                objGraphics.DrawLine(
                    bluePen,
                    x, chartBottom - valueY1,
                    x + lineLength, chartBottom - valueY2);

                objGraphics.FillEllipse(
                    blueBrush,
                    x + lineLength - 3, chartBottom - valueY2 - 3,
                    6, 6);

                // draw x axis labels

                var xVal = string.Empty;

                try
                {
                    xVal = Convert.ToString((int)dataTable.Rows[i][0]);
                }
                catch (Exception)
                {
                    xVal = Convert.ToString(dataTable.Rows[i][0]);
                }

                if (x - prevXAxisLabel > 50) // space them apart, so they don't bump into each other
                {
                    // the little line so that the label points to the the data point
                    objGraphics.DrawLine(
                        blackPen,
                        x, chartBottom,
                        x, chartBottom + 14);

                    objGraphics.DrawString(
                        xVal,
                        fontLegend,
                        blackBrush,
                        x, xAxisTextY);

                    prevXAxisLabel = x;
                }

                x += lineLength;
            }

            return objBitmap;
        }

        private static Bitmap WriteNoDataMessage(string title, int scale)
        {
            var chartWidth = 640 / scale;
            var chartHeight = 300 / scale;
            var chartTopMargin = 10 / scale; // gap between highest bar and border of chart

            var xAxisTextOffset = 8 / scale; // gap between edge and start of x axis text
            var pageTopMargin = 40 / scale; // gape between chart and top of page

            var fontTitle = new Font("Arial", 12, FontStyle.Bold);
            var fontLegend = new Font("Arial", 8);
            var pageBottomMargin = 3 * fontLegend.Height;
            var pageLeftMargin = 4 * fontLegend.Height + xAxisTextOffset; // where the y axis text goes

            // Create a Bitmap instance
            var objBitmap = new Bitmap(
                pageLeftMargin + chartWidth, // total width
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // total height

            using var objGraphics = Graphics.FromImage(objBitmap);
            using var blackBrush = new SolidBrush(Color.Black);
            using var whiteBrush = new SolidBrush(Color.White);

            // white overall background
            objGraphics.FillRectangle(
                whiteBrush,
                0, 0,
                pageLeftMargin + chartWidth, // far left
                pageTopMargin + fontTitle.Height + chartHeight + pageBottomMargin); // bottom

            // draw title
            objGraphics.DrawString(
                title + " (no data to chart)",
                fontTitle,
                blackBrush,
                xAxisTextOffset,
                fontTitle.Height / 2);

            return objBitmap;
        }
    }
}
