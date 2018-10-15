using ChartJSCore.Models;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface IChartService
    {
        Chart CreateBarChart(TfsQueryResult queryResult);
    }
}