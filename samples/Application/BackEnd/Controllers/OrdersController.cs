﻿using System.Buffers;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Micronetes;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpPost]
        public async Task Post([FromServices]IClientFactory<Channel<byte[]>> channelFactory)
        {
            var orders = channelFactory.CreateClient("queue/orders");

            var reader = Request.BodyReader;
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                if (result.IsCompleted)
                {
                    await orders.Writer.WriteAsync(buffer.ToArray());
                    reader.AdvanceTo(buffer.End);
                    break;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }
    }
}
