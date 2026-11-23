namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppointmentId = c.Int(nullable: false),
                        PaymentMethod = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId, cascadeDelete: true)
                .Index(t => t.AppointmentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "AppointmentId", "dbo.Appointments");
            DropIndex("dbo.Payments", new[] { "AppointmentId" });
            DropTable("dbo.Payments");
        }
    }
}
