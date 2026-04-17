using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Transport.Model;

namespace Transport.Repository
{
    public interface IInvoiceRepository
    {
        long SaveInvoice(InvoiceHeaderModel header, List<InvoiceDetailModel> details);
        List<InvoiceHeaderModel> GetAllInvoices(string customerName, string invoiceNo, DateTime? fromDate, DateTime? toDate);
        InvoiceHeaderModel GetInvoiceById(long invoiceId);
        List<InvoiceDetailModel> GetInvoiceDetails(long invoiceId);
        bool DeleteInvoice(long invoiceId);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private string GetConnectionString()
        {
            // Same pattern used by ReportsController - reads raw connection string from ConnectionString.txt
            string txtpath = System.Web.Hosting.HostingEnvironment.MapPath("~/ConnectionString.txt");
            string connectionValue = "";
            using (System.IO.StreamReader sr = new System.IO.StreamReader(txtpath))
            {
                while (sr.Peek() >= 0)
                    connectionValue = sr.ReadLine();
            }
            return connectionValue;
        }

        public long SaveInvoice(InvoiceHeaderModel header, List<InvoiceDetailModel> details)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Generate invoice number
                        string invoiceNo = header.InvoiceNo;
                        if (string.IsNullOrEmpty(invoiceNo))
                        {
                            var numCmd = new SqlCommand(
                                "SELECT 'NTT-INV-' + RIGHT('00000' + CAST(ISNULL(MAX(InvoiceID),0)+1 AS VARCHAR), 5) FROM InvoiceHeaders",
                                conn, tran);
                            invoiceNo = numCmd.ExecuteScalar()?.ToString() ?? "NTT-INV-00001";
                        }

                        // Insert header
                        var headerCmd = new SqlCommand(@"
                            INSERT INTO InvoiceHeaders 
                                (InvoiceNo, InvoiceDate, CustomerName, JobVendorCode, JobVendorName,
                                 DrivingBy, DrivingByName, VehicleCode, VehicleName, CashInHand, CashInHandName,
                                 StartDate, EndDate, CreditCash, TotalAmount, IsCredit, CreatedBy, CreatedDate)
                            VALUES
                                (@InvoiceNo, @InvoiceDate, @CustomerName, @JobVendorCode, @JobVendorName,
                                 @DrivingBy, @DrivingByName, @VehicleCode, @VehicleName, @CashInHand, @CashInHandName,
                                 @StartDate, @EndDate, @CreditCash, @TotalAmount, @IsCredit, @CreatedBy, GETDATE());
                            SELECT SCOPE_IDENTITY();", conn, tran);

                        headerCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                        headerCmd.Parameters.AddWithValue("@InvoiceDate", DateTime.Now);
                        headerCmd.Parameters.AddWithValue("@CustomerName", (object)header.CustomerName ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@JobVendorCode", (object)header.JobVendorCode ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@JobVendorName", (object)header.JobVendorName ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@DrivingBy", (object)header.DrivingBy ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@DrivingByName", (object)header.DrivingByName ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@VehicleCode", (object)header.VehicleCode ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@VehicleName", (object)header.VehicleName ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@CashInHand", (object)header.CashInHand ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@CashInHandName", (object)header.CashInHandName ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@StartDate", (object)header.StartDate ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@EndDate", (object)header.EndDate ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@CreditCash", (object)header.CreditCash ?? DBNull.Value);
                        headerCmd.Parameters.AddWithValue("@TotalAmount", header.TotalAmount);
                        headerCmd.Parameters.AddWithValue("@IsCredit", header.IsCredit);
                        headerCmd.Parameters.AddWithValue("@CreatedBy", (object)header.CreatedBy ?? DBNull.Value);

                        long newInvoiceId = Convert.ToInt64(headerCmd.ExecuteScalar());

                        // Insert details
                        foreach (var detail in details)
                        {
                            var detailCmd = new SqlCommand(@"
                                INSERT INTO InvoiceDetails
                                    (InvoiceID, JobCode, JobDate, JobTime, JobFrom, JobTo,
                                     CustomerName, VehicleName, DrivingByName, JobVendorName,
                                     Credit, Cash, Amount)
                                VALUES
                                    (@InvoiceID, @JobCode, @JobDate, @JobTime, @JobFrom, @JobTo,
                                     @CustomerName, @VehicleName, @DrivingByName, @JobVendorName,
                                     @Credit, @Cash, @Amount)", conn, tran);

                            detailCmd.Parameters.AddWithValue("@InvoiceID", newInvoiceId);
                            detailCmd.Parameters.AddWithValue("@JobCode", detail.JobCode);
                            detailCmd.Parameters.AddWithValue("@JobDate", (object)detail.JobDate ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@JobTime", (object)detail.JobTime ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@JobFrom", (object)detail.JobFrom ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@JobTo", (object)detail.JobTo ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@CustomerName", (object)detail.CustomerName ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@VehicleName", (object)detail.VehicleName ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@DrivingByName", (object)detail.DrivingByName ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@JobVendorName", (object)detail.JobVendorName ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@Credit", (object)detail.Credit ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@Cash", (object)detail.Cash ?? DBNull.Value);
                            detailCmd.Parameters.AddWithValue("@Amount", (object)detail.Amount ?? DBNull.Value);
                            detailCmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return newInvoiceId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<InvoiceHeaderModel> GetAllInvoices(string customerName, string invoiceNo, DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<InvoiceHeaderModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT h.*, 
                           (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID = h.InvoiceID) AS TotalJobs
                    FROM InvoiceHeaders h
                    WHERE 1=1
                      AND (@CustomerName IS NULL OR h.CustomerName LIKE '%' + @CustomerName + '%')
                      AND (@InvoiceNo IS NULL OR h.InvoiceNo LIKE '%' + @InvoiceNo + '%')
                      AND (@FromDate IS NULL OR CAST(h.InvoiceDate AS DATE) >= CAST(@FromDate AS DATE))
                      AND (@ToDate IS NULL OR CAST(h.InvoiceDate AS DATE) <= CAST(@ToDate AS DATE))
                    ORDER BY h.InvoiceID DESC", conn);

                cmd.Parameters.AddWithValue("@CustomerName", string.IsNullOrEmpty(customerName) ? (object)DBNull.Value : customerName);
                cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(invoiceNo) ? (object)DBNull.Value : invoiceNo);
                cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapHeader(reader));
                    }
                }
            }
            return list;
        }

        public InvoiceHeaderModel GetInvoiceById(long invoiceId)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT h.*, 
                           (SELECT COUNT(*) FROM InvoiceDetails d WHERE d.InvoiceID = h.InvoiceID) AS TotalJobs
                    FROM InvoiceHeaders h 
                    WHERE h.InvoiceID = @InvoiceID", conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return MapHeader(reader);
                }
            }
            return null;
        }

        public List<InvoiceDetailModel> GetInvoiceDetails(long invoiceId)
        {
            var list = new List<InvoiceDetailModel>();
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT * FROM InvoiceDetails WHERE InvoiceID = @InvoiceID ORDER BY JobDate, JobTime", conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new InvoiceDetailModel
                        {
                            InvoiceDetailID = Convert.ToInt64(reader["InvoiceDetailID"]),
                            InvoiceID = Convert.ToInt64(reader["InvoiceID"]),
                            JobCode = Convert.ToInt64(reader["JobCode"]),
                            JobDate = reader["JobDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["JobDate"]),
                            JobTime = reader["JobTime"]?.ToString(),
                            JobFrom = reader["JobFrom"]?.ToString(),
                            JobTo = reader["JobTo"]?.ToString(),
                            CustomerName = reader["CustomerName"]?.ToString(),
                            VehicleName = reader["VehicleName"]?.ToString(),
                            DrivingByName = reader["DrivingByName"]?.ToString(),
                            JobVendorName = reader["JobVendorName"]?.ToString(),
                            Credit = reader["Credit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Credit"]),
                            Cash = reader["Cash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Cash"]),
                            Amount = reader["Amount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Amount"]),
                        });
                    }
                }
            }
            return list;
        }

        public bool DeleteInvoice(long invoiceId)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        new SqlCommand("DELETE FROM InvoiceDetails WHERE InvoiceID = @ID", conn, tran)
                        { Parameters = { new SqlParameter("@ID", invoiceId) } }.ExecuteNonQuery();
                        new SqlCommand("DELETE FROM InvoiceHeaders WHERE InvoiceID = @ID", conn, tran)
                        { Parameters = { new SqlParameter("@ID", invoiceId) } }.ExecuteNonQuery();
                        tran.Commit();
                        return true;
                    }
                    catch { tran.Rollback(); return false; }
                }
            }
        }

        private InvoiceHeaderModel MapHeader(SqlDataReader r)
        {
            return new InvoiceHeaderModel
            {
                InvoiceID = Convert.ToInt64(r["InvoiceID"]),
                InvoiceNo = r["InvoiceNo"]?.ToString(),
                InvoiceDate = Convert.ToDateTime(r["InvoiceDate"]),
                CustomerName = r["CustomerName"]?.ToString(),
                JobVendorCode = r["JobVendorCode"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["JobVendorCode"]),
                JobVendorName = r["JobVendorName"]?.ToString(),
                DrivingBy = r["DrivingBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["DrivingBy"]),
                DrivingByName = r["DrivingByName"]?.ToString(),
                VehicleCode = r["VehicleCode"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["VehicleCode"]),
                VehicleName = r["VehicleName"]?.ToString(),
                CashInHand = r["CashInHand"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["CashInHand"]),
                CashInHandName = r["CashInHandName"]?.ToString(),
                StartDate = r["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["StartDate"]),
                EndDate = r["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDate"]),
                CreditCash = r["CreditCash"]?.ToString(),
                TotalAmount = Convert.ToDecimal(r["TotalAmount"]),
                IsCredit = Convert.ToBoolean(r["IsCredit"]),
                CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                TotalJobs = r["TotalJobs"] == DBNull.Value ? 0 : Convert.ToInt32(r["TotalJobs"])
            };
        }
    }
}