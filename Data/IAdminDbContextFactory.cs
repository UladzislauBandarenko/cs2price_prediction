namespace cs2price_prediction.Data
{
    public interface IAdminDbContextFactory
    {
        AppDbContext CreateAdminContext();
    }
}
