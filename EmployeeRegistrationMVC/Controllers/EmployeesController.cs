using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeRegistrationMVC.Context;
using EmployeeRegistrationMVC.Models;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace EmployeeRegistrationMVC.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeContext _context;
        public static IConfigurationRoot Configuration { get; set; }

        public EmployeesController(EmployeeContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchTerm)
        {
            var employees = await _context.Employee.FromSqlRaw("SELECT * FROM dbo.Employee order by HireDate").ToListAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                employees = employees.Where(e => e.LastName.Contains(searchTerm)
                                              || e.Phone.Contains(searchTerm))
                                     .ToList();
            }

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,Phone,Zip,HireDate")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                //_context.Add(employee);
                var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                Configuration = builder.Build();

                string connectionString = Configuration.GetConnectionString("MyContextConnection");

                using (SqlConnection openCon = new SqlConnection(connectionString))
                {
                    string saveEmployee = "INSERT INTO [dbo].[Employee] ([LastName],[FirstName],[Phone],[Zip],[HireDate]) VALUES (@lastName,@firstName,@phone,@zip,@hireDate)";

                    using (SqlCommand querySaveEmployee = new SqlCommand(saveEmployee))
                    {
                        querySaveEmployee.Connection = openCon;
                        querySaveEmployee.Parameters.Add("@lastName", SqlDbType.VarChar, 30).Value = employee.LastName;
                        querySaveEmployee.Parameters.Add("@firstName", SqlDbType.VarChar, 30).Value = employee.FirstName;
                        querySaveEmployee.Parameters.Add("@phone", SqlDbType.VarChar, 30).Value = employee.Phone;
                        querySaveEmployee.Parameters.Add("@zip", SqlDbType.VarChar, 30).Value = employee.Zip;
                        querySaveEmployee.Parameters.Add("@hireDate", SqlDbType.VarChar, 30).Value = employee.HireDate;

                        openCon.Open();

                        querySaveEmployee.ExecuteNonQuery();
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,Phone,Zip,HireDate")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}
