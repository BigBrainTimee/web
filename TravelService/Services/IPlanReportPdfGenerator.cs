using TravelService.Dtos;

namespace TravelService.Services;

public interface IPlanReportPdfGenerator
{
    byte[] Generate(TravelPlanReportDto report);
}
