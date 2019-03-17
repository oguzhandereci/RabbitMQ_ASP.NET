using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.BLL.RabbitMq;
using RabbitMQ.BLL.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MODELS.Entities;

namespace RabbitMQ.Consumer
{
    public class Consumer
    {
        private readonly RabbitMqService _rabbitMqService;
        public EventingBasicConsumer ConsumerEvent;
        public Form1 form { get; set; }

        public Consumer(string queueName)
        {
            _rabbitMqService = new RabbitMqService();
            var conn = _rabbitMqService.GetRabbitMqConnection();
            var channel = conn.CreateModel();
            ConsumerEvent = new EventingBasicConsumer(channel);

            ConsumerEvent.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                //var data = $"{queueName} isimli queue üzerinden gelen mesaj: \"{message}\"";
                if (queueName == "MailLog")
                {
                    var data = JsonConvert.DeserializeObject<MailLog>(message);
                    //işlemler
                    new MailLogRepo().Insert(new MailLog()
                    {
                        Id = data.Id,
                        CustomerId = data.CustomerId,
                        Message = data.Message,
                        Subject = data.Subject
                    });
                    Form1.logMailLog++;
                    form.Text = $"Customer {Form1.logCustomer} - MailLog {Form1.logMailLog}";
                }
                else if (queueName == "Customer")
                {
                    var data = JsonConvert.DeserializeObject<List<Customer>>(message);
                    var repo = new CustomerRepo();
                    for (var i = 0; i < data.Count; i++)
                    {
                        var item = data[i];
                        Form1.logCustomer++;
                        form.Text = $"Customer {Form1.logCustomer} - MailLog {Form1.logMailLog}";
                        repo.InsertForMark(new Customer()
                        {
                            Address = item.Address,
                            Email = item.Email,
                            Id = item.Id,
                            Name = item.Name,
                            Phone = item.Phone,
                            Surname = item.Surname,
                            RegisterDate = item.RegisterDate
                        });
                        if (i % 100 == 0)
                            repo.Save();
                    }
                    repo.Save();

                    //işlemler
                }


            };
            channel.BasicConsume(queueName, true, ConsumerEvent);
        }
    }
}
