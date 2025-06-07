using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.Context;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContractController(ApplicationDbContext context)
        {
            _context = context;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContract()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .ToListAsync();

            if (contracts == null || !contracts.Any())
            {
                return NotFound(new { message = "No contracts found." });
            }

            return Ok(new
            {
                message = "Contracts retrieved successfully",
                data = contracts.Select(c => new
                {
                    c.ContractId,
                    c.TenantId,
                    TenantName = c.Tenant != null ? c.Tenant.FullName : null,
                    c.RoomId,
                    RoomNumber = c.Room != null ? c.Room.RoomNumber : null,
                    c.StartDate,
                    c.EndDate,
                    c.DepositAmount,
                    c.PdfFileName,
                    c.IsActive,
                })
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenantById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null)
            {
                return NotFound(new { message = "Contract not found." });
            }

            return Ok(new
            {
                message = "Contract retrieved successfully",
                data = new
                {
                    contract.ContractId,
                    contract.TenantId,
                    TenantName = contract.Tenant != null ? contract.Tenant.FullName : null,
                    contract.RoomId,
                    RoomNumber = contract.Room != null ? contract.Room.RoomNumber : null,
                    contract.StartDate,
                    contract.EndDate,
                    contract.DepositAmount,
                    contract.PdfFileName,
                    contract.IsActive,
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] Contract contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid contract data", errors = ModelState });
            }

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts");
            Directory.CreateDirectory(folderPath);
           
            var contracts = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.ContractId == contract.ContractId);

            if (contracts == null || contracts.Tenant == null || contracts.Room == null)
            {
                return StatusCode(500, new { message = "Tenant or Room information is missing for the contract." });
            }

            var fileName = $"contract_{contract.ContractId}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            try
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        var paddingLeft = 45;
                        var paddingRight = 45;

                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x
                            .FontFamily("THSarabunNew")
                            .FontSize(14)
                            .LineHeight(1.5f)
                        );

                        page.Content().Column(col =>
                        {
                            col.Item().AlignCenter().PaddingTop(20).PaddingBottom(20).PaddingRight(paddingRight)
                                .Text("แบบสัญญาเช่าหอพัก").FontSize(18).Bold();

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("");
                                row.ConstantItem(200).AlignRight().Column(rightCol =>
                                {
                                    rightCol.Item().Text("ทำที่ ..........................");
                                    var today = DateTime.Now;
                                    rightCol.Item().Text($"วันที่ {today:dd MMMM yyyy}");
                                });
                            });

                            col.Item().PaddingLeft(paddingLeft + 50).PaddingBottom(5).PaddingTop(15)
                                .Text($"สัญญาเช่าหอพักฉบับนี้ทำขึ้นระหว่าง (นาย/นาง/น.ส.) {contracts.Tenant.FullName}");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5).Text(text =>
                            {
                                text.Span($"เลขที่บัตรประชาชน {contracts.Tenant.IDCardNumber} ");
                                text.Span("หมายเลขทะเบียนนิติบุคคล..................................");
                            });

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5).Text(text =>
                            {
                                text.Span($"บ้านเลขที่ {contracts.Tenant.HouseNumber} ");
                                text.Span("ตรอก/ซอย.......................... ");
                                text.Span("ถนน.......................... ");
                                text.Span($"ตำบล/แขวง {contracts.Tenant.SubDistrict} ");
                            });

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5).Text(text =>
                            {
                                text.Span($"อำเภอ/เขต {contracts.Tenant.District} ");
                                text.Span($"จังหวัด {contracts.Tenant.Province}");
                                text.Span($"รหัสไปรษณีย์ {contracts.Tenant.PostalCode} ");
                                text.Span($"เบอร์โทรศัพท์ {contracts.Tenant.PhoneNumber} ");
                            });

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("โทรศัพท์มือถือ................................ ซึ่งต่อไปในสัญญาเช่าหอพักนี้จะเรียกว่า \"ผู้เช่า\" ฝ่ายหนึ่ง");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5).Text(text =>
                            {
                                text.Span($"กับ ...........................................................  ");
                                text.Span("เลขที่บัตรประชาชน .............................................");

                            });
                           
                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("บ้านเลขที่ .................... ตรอก/ซอย .................... ถนน .................... ตำบล/แขวง ....................");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("อำเภอ/เขต .................... จังหวัด .................... รหัสไปรษณีย์ ....................");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("เบอร์โทรศัพท์ .................... ซึ่งต่อไปในสัญญาเช่าหอพักนี้จะเรียกว่า \"ผู้ให้เช่า\" อีกฝ่ายหนึ่ง");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5).Text(text =>
                            {
                                text.Span($"โดยมีรายละเอียดการเช่าดังนี้");
                                text.Span($"หมายเลขห้อง {contracts.Room.RoomNumber}");
                                text.Span($"วันที่เริ่มสัญญา {contracts.StartDate : dd/mm/yyyy}");
                                text.Span($"วันที่สิ้นสุดสัญญา {contracts.EndDate : dd/mm/yyyy}");
                            });

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("เงื่อนไขการเช่า").FontSize(16).Bold();

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("1. ผู้เช่าต้องชำระค่าเช่าล่วงหน้าทุกวันที่ 1 ของเดือน");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("2. ผู้เช่าต้องรักษาความสะอาดและความเป็นระเบียบเรียบร้อยของห้องพัก");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("3. ห้ามนำสัตว์เลี้ยงเข้าพักในหอพัก");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("4. ห้ามจัดงานเลี้ยงหรือทำกิจกรรมที่ก่อให้เกิดเสียงดังรบกวน");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("5. ผู้เช่าต้องแจ้งล่วงหน้า 30 วัน หากต้องการยกเลิกสัญญา");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("6. หากผู้เช่าผิดสัญญา ผู้ให้เช่ามีสิทธิ์บอกเลิกสัญญาได้ทันที");

                            col.Item().PaddingLeft(paddingLeft).PaddingBottom(5)
                                .Text("7. เงินประกันจะคืนให้หลังจากตรวจสอบห้องพักแล้วไม่มีความเสียหาย");

                            col.Item().PaddingTop(10).PaddingBottom(10)
                                .Text("ทั้งสองฝ่ายได้อ่านและเข้าใจข้อความในสัญญานี้โดยตลอดแล้ว จึงได้ลงลายมือชื่อไว้เป็นหลักฐาน");

                            col.Item().PaddingTop(20).Row(row =>
                            {
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().AlignCenter().Text("ลงชื่อ .....................................");
                                    leftCol.Item().AlignCenter().PaddingTop(5).Text("(............................................)");
                                    leftCol.Item().AlignCenter().PaddingTop(5).Text("ผู้เช่า");
                                    leftCol.Item().AlignCenter().PaddingTop(10).Text("วันที่ ........./........./.........");
                                });

                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignCenter().Text("ลงชื่อ .....................................");
                                    rightCol.Item().AlignCenter().PaddingTop(5).Text("(............................................)");
                                    rightCol.Item().AlignCenter().PaddingTop(5).Text("ผู้ให้เช่า");
                                    rightCol.Item().AlignCenter().PaddingTop(10).Text("วันที่ ........./........./.........");
                                });
                            });

                            col.Item().PaddingTop(209).Row(row =>
                            {
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().AlignCenter().Text("ลงชื่อ .....................................");
                                    leftCol.Item().AlignCenter().PaddingTop(5).Text("(............................................)");
                                    leftCol.Item().AlignCenter().PaddingTop(5).Text("พยาน");
                                    leftCol.Item().AlignCenter().PaddingTop(10).Text("วันที่ ........./........./.........");
                                });

                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignCenter().Text("ลงชื่อ .....................................");
                                    rightCol.Item().AlignCenter().PaddingTop(5).Text("(............................................)");
                                    rightCol.Item().AlignCenter().PaddingTop(5).Text("พยาน");
                                    rightCol.Item().AlignCenter().PaddingTop(10).Text("วันที่ ........./........./.........");
                                });
                            });
                        });
                    });
                }).GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate PDF", error = ex.Message });
            }

            contract.PdfFileName = fileName;
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateContract), new { id = contract.ContractId }, new
            {
                message = "Contract created and PDF generated",
                data = new
                {
                    contract.ContractId,
                    contract.TenantId,
                    TenantName = contract.Tenant?.FullName,
                    TenantEmail = contract.Tenant?.Email,
                    TenantPhone = contract.Tenant?.PhoneNumber,
                    contract.RoomId,
                    RoomNumber = contract.Room?.RoomNumber,
                    RoomType = contract.Room?.RoomType,
                    RoomPrice = contract.Room?.Price,
                    contract.StartDate,
                    contract.EndDate,
                    contract.DepositAmount,
                    contract.PdfFileName,
                    contract.IsActive
                },
                pdfUrl = $"/contracts/{fileName}"
            });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> EditIsActive(int id, [FromBody] Contract contract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid contract data", errors = ModelState });
            }
            var existingContract = await _context.Contracts.FindAsync(id);
            if (existingContract == null)
            {
                return NotFound(new { message = "Contract not found." });
            }
            existingContract.IsActive = contract.IsActive;
            _context.Contracts.Update(existingContract);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Contract status updated successfully",
                data = new
                {
                    existingContract.ContractId,
                    existingContract.IsActive
                }
            });
        }
    }
}