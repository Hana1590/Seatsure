using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatsure.Application.Bll.DTOs;
using Seatsure.Application.DTOs;
using Seatsure.Domain;

namespace Seatsure.Application.Services.Interfaces;
public  interface IEventService
{

    
    Task<PagedResult<EventDto>> ListPublishedEventsAsync(int page, int pageSize);


    Task<EventDetailsDto> GetEventByIdAsync(Guid id); 

    
    Task<EventDto> CreateEventAsync(Guid CallerId,  CreateEventDto request);

    Task<EventDto> PublishEventAsync(Guid CallerId, Guid EventId);

}
