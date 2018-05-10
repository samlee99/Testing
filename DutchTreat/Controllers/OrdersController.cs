using AutoMapper;
using DutchTreat.Data;
using DutchTreat.Data.Entities;
using DutchTreat.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchTreat.Controllers
{
    [Route("api/[Controller]")]
    [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : Controller
    {
        private readonly IDutchRepository repository;
        private readonly ILogger<OrdersController> logger;
        private readonly IMapper mapper;
        private readonly UserManager<StoreUser> userManager;

        public OrdersController(IDutchRepository repository, 
            ILogger<OrdersController> logger,
            IMapper mapper,
            UserManager<StoreUser> userManager)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet()]
        public IActionResult Get(bool includeItems = true)
        {
            try
            {
                var username = User.Identity.Name;

                var results = repository.GetAllOrdersByUser(username, includeItems);

                return Ok(mapper.Map<IEnumerable<Order>, IEnumerable<OrderViewModel>>(results));
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to get orders: {e}");
                return BadRequest("Failed to get orders");
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            try
            {
                var order = repository.GetOrderById(User.Identity.Name, id);
                if (order != null) return Ok(mapper.Map<Order, OrderViewModel>(order));
                else return NotFound();
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to get orders: {e}");
                return BadRequest("Failed to get orders");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] OrderViewModel model)
        {
            // add it to the db
            try
            {
                if(ModelState.IsValid)
                {
                    var newOrder = mapper.Map<OrderViewModel, Order>(model);

                    //var newOrder = new Order()
                    //{
                    //    OrderDate = model.OrderDate,
                    //    OrderNumber = model.OrderNumber,
                    //    Id = model.OrderId
                    //};

                    if (newOrder.OrderDate == DateTime.MinValue)
                    {
                        newOrder.OrderDate = DateTime.Now;
                    }

                    var currentUser = await userManager.FindByNameAsync(User.Identity.Name);
                    newOrder.User = currentUser;

                    repository.AddEntity(newOrder);

                    if (repository.SaveAll())
                    {
                        //var vm = mapper.Map<Order, OrderViewModel>(newOrder);

                        //var vm = new OrderViewModel()
                        //{
                        //    OrderId = newOrder.Id,
                        //    OrderNumber = newOrder.OrderNumber,
                        //    OrderDate = newOrder.OrderDate
                        //};

                        return Created($"/api/orders/{newOrder.Id}", mapper.Map<Order, OrderViewModel>(newOrder));
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
                
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to save a new order: {e}");
            }

            return BadRequest("Failed to save a new order");
        }
    }
}