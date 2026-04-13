using System.Threading.Tasks;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IOutboxPublisherJob
{
    Task ProcessOutboxMessagesAsync();
}
