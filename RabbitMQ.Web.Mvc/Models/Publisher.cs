using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RabbitMQ.BLL.RabbitMq;
using RabbitMQ.Client;

namespace RabbitMQ.Web.Mvc.Models
{
    public class Publisher
    {
        private readonly RabbitMqService _rabbitMqService;
        private const string DefaultQueue = "wissen1";
        public Publisher(string message, string queueName = null)
        {
            if (string.IsNullOrEmpty(queueName))
                queueName = DefaultQueue;
            _rabbitMqService = new RabbitMqService();

            using (var conn = _rabbitMqService.GetRabbitMqConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(queueName,false,false,false,null);
                    channel.BasicPublish(String.Empty,queueName,null,Encoding.UTF8.GetBytes(message));
                }
            }
        }
    }
}