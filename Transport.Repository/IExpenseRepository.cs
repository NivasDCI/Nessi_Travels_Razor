using Transport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Repository
{
    public interface IExpenseRepository
    {
        List<ExpenseModel> Expense_FindAll(int? page, int? limit, string sortBy, DateTime? StartDate, DateTime? EndDate, string direction, out int TotalCount);
        ExpenseModel Expense_Edit(long ExpenseCode);
        ReturnMessageModel Expense_Save(ExpenseModel objExpense);
        ReturnMessageModel Expense_Insert(ExpenseModel objExpense);
        ReturnMessageModel Expense_Update(ExpenseModel objExpense);
        ReturnMessageModel Expense_Delete(long ExpenseCode);

        decimal? GetSupplierCost(long suppliertypecode);
        List<ExpenseModel> MyExpense_FindAll(int? page, int? limit, string sortBy, int? PaymentBy, DateTime? StartDate, DateTime? EndDate, string direction, out int TotalCount);
        ExpenseModel MyExpense_Edit(long ExpenseCode);
        ReturnMessageModel MyExpense_Save(ExpenseModel objExpense);
        ReturnMessageModel MyExpense_Insert(ExpenseModel objExpense);
        ReturnMessageModel MyExpense_Update(ExpenseModel objExpense);
        ReturnMessageModel MyExpense_Delete(long ExpenseCode);

    }
}
