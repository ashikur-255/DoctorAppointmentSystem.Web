namespace DoctorAppointmentSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePaymentModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "TransactionId", c => c.String());
            AddColumn("dbo.Payments", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "Status");
            DropColumn("dbo.Payments", "TransactionId");
        }
    }
}
