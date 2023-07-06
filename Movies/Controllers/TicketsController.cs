using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.Identity.UI.V5.Pages.Account.Manage.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using Org.BouncyCastle.Asn1.Crmf;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.IO;
using ClosedXML.Excel;

namespace Movies.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
              return _context.TicketsDb != null ? 
                          View(await _context.TicketsDb.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.TicketsDb'  is null.");
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketsDb == null)
            {
                return NotFound();
            }

            var tickets = await _context.TicketsDb
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tickets == null)
            {
                return NotFound();
            }

            return View(tickets);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieTitle,Author,Genre,NumTickets,Date")] Tickets tickets)
        {

            if (ModelState.IsValid)
            {
                _context.Add(tickets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));


            }
            return View(tickets);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketsDb == null)
            {
                return NotFound();
            }

            var tickets = await _context.TicketsDb.FindAsync(id);
            if (tickets == null)
            {
                return NotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieTitle,Author,Genre,NumTickets,Date")] Tickets tickets)
        {
            if (id != tickets.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tickets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketsExists(tickets.Id))
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
            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketsDb == null)
            {
                return NotFound();
            }

            var tickets = await _context.TicketsDb
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tickets == null)
            {
                return NotFound();
            }

            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketsDb == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketsDb'  is null.");
            }
            var tickets = await _context.TicketsDb.FindAsync(id);
            if (tickets != null)
            {
                _context.TicketsDb.Remove(tickets);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cart()
        {
            var cartItems = await _context.CartDb.Include(c => c.Ticket).ToListAsync();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            Tickets ticket = await _context.TicketsDb.FindAsync(id);
            if (ticket != null)
            {
                Cart cart = new Cart();
                cart.TicketId = ticket.Id;
                cart.Ticket = ticket;
                _context.CartDb.Add(cart);

                ticket.NumTickets--;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Orders()
        {
            var orderItems = await _context.OrdersDb.Include(c => c.FromCart).ToListAsync();
            return View(orderItems);
        }

        public async Task<IActionResult> BuyAll()
        {

            MailAddress to = new MailAddress(User.Identity.Name);
            MailAddress from = new MailAddress("automaticemail24@gmail.com");
            Orders order = new Orders();
            MailMessage message = new MailMessage(from, to);
            message.Subject = "You just made an order";
            message.Body = "You just purchased these movies ";
            var cartItems = await _context.CartDb.Include(c => c.Ticket).ToListAsync();
            foreach (var item in cartItems)
            {
                order = new Orders();
                message.Body += _context.TicketsDb.Find(item.TicketId).MovieTitle.ToString();
                message.Body += ", ";
                order.TicketId = item.TicketId;
                order.FromCart = item.Ticket;
                _context.OrdersDb.Add(order);
                _context.CartDb.Remove(item);
                await _context.SaveChangesAsync();
            }

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("automaticemail24@gmail.com", "nipkgqblwtcfxplp"),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> BuyItem (int id, int TicketId)
        {
            MailAddress to = new MailAddress(User.Identity.Name);
            MailAddress from = new MailAddress("automaticemail24@gmail.com");
            Tickets ticket = await _context.TicketsDb.FindAsync(TicketId);
            MailMessage message = new MailMessage(from, to);
            message.Subject = "You just made an order";
            message.Body = "You just purchased " + ticket.MovieTitle;
            Orders order = new Orders();
            order.TicketId = ticket.Id;
            order.FromCart = ticket;
            _context.OrdersDb.Add(order);
            _context.CartDb.Remove(_context.CartDb.Find(id));
            await _context.SaveChangesAsync();
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("automaticemail24@gmail.com", "nipkgqblwtcfxplp"),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("Cart");
        }
        
        public async Task<IActionResult> CreatePDFDocument()
        {
            //Create a new PDF document.
            PdfDocument document = new PdfDocument();
            //Add a page to the document.
            PdfPage page = document.Pages.Add();
            //Create PDF graphics for the page.
            PdfGraphics graphics = page.Graphics;
            //Set the standard font.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
            //Draw the text.
            var orders = "";
            var orderedItems = await _context.OrdersDb.Include(c => c.FromCart).ToListAsync();
            foreach (var item in orderedItems)
            {
                orders += item.FromCart.MovieTitle + "<br>";
            }
            graphics.DrawString(orders, font, PdfBrushes.Black, new PointF(0, 0));
            //Saving the PDF to the MemoryStream.
            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            //Set the position as '0'.
            stream.Position = 0;
            //Download the PDF document in the browser.
            FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/pdf");
            fileStreamResult.FileDownloadName = "Sample.pdf";
            return fileStreamResult;
        }

        public async Task<IActionResult> AddToRole(int id)
        {
            return View();
        }

        public async Task<IActionResult> ExcelGenre ()
        {
            return View();
        }

        public IActionResult Excel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Id";
                worksheet.Cell(currentRow, 2).Value = "MovieTitle";
                worksheet.Cell(currentRow, 3).Value = "Author";
                worksheet.Cell(currentRow, 4).Value = "Genre";
                worksheet.Cell(currentRow, 5).Value = "NumTickets";
                worksheet.Cell(currentRow, 6).Value = "Date";
                foreach (var user in _context.TicketsDb.ToList())
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.Id;
                    worksheet.Cell(currentRow, 2).Value = user.MovieTitle;
                    worksheet.Cell(currentRow, 3).Value = user.Author;
                    worksheet.Cell(currentRow, 4).Value = user.Genre;
                    worksheet.Cell(currentRow, 5).Value = user.NumTickets;
                    worksheet.Cell(currentRow, 6).Value = user.Date;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "users.xlsx");
                }
            }
        }

        private bool TicketsExists(int id)
        {
          return (_context.TicketsDb?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}