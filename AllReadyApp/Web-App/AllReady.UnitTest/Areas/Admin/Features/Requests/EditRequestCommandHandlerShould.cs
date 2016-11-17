﻿using AllReady.Areas.Admin.Features.Requests;
using AllReady.Areas.Admin.ViewModels.Request;
using AllReady.Models;
using AllReady.Providers;
using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Geocoding;
using System.Collections.Generic;

namespace AllReady.UnitTest.Areas.Admin.Features.Requests
{
    public class EditRequestCommandHandlerShould : InMemoryContextTest
    {
        private Request _existingRequest;
        protected override void LoadTestData()
        {
            _existingRequest = new Request
            {
                Address = "1234 Nowhereville",
                City = "Seattle",
                Name = "Request unit test",
                DateAdded = DateTime.MinValue,
                EventId = 1,
                Phone = "555-555-5555",
                Email = "something@example.com",
                State = "WA",
                Zip = "55555",
                Latitude = 10,
                Longitude = 10
            };

            Context.Requests.Add(_existingRequest);
            Context.SaveChanges();
        }

        [Fact]
        public async Task ReturnNewRequestIdOnSuccessfulCreation()
        {
            var handler = new EditRequestCommandHandler(Context, new NullObjectGeocoder());
            var requestId = await handler.Handle(new EditRequestCommand { RequestModel = new EditRequestViewModel {  } });

            Assert.NotEqual(Guid.Empty, requestId);
        }

        [Fact]
        public async Task UpdateRequestsThatAlreadyExisted()
        {
            string expectedName = "replaced name";

            var handler = new EditRequestCommandHandler(Context, new NullObjectGeocoder());
            await handler.Handle(new EditRequestCommand
            {
                RequestModel = new EditRequestViewModel { Id = _existingRequest.RequestId, Name = expectedName }
            });

            var request = Context.Requests.First(r => r.RequestId == _existingRequest.RequestId);
            Assert.Equal(expectedName, request.Name );
        }

        [Fact]
        public async Task AlwaysGeocodeAddressWhenUpdatingExistingRequest()
        {
            var mockGeocoder = new Mock<IGeocoder>();

            // Because the Geocode method takes a set of strings as arguments, verify the arguments are passed in to the mock the correct order.
            mockGeocoder.Setup(g => g.Geocode(_existingRequest.Address, _existingRequest.City, _existingRequest.State, _existingRequest.Zip, It.IsAny<string>())).Returns(new List<Address>());

            var handler = new EditRequestCommandHandler(Context, mockGeocoder.Object);
            await handler.Handle(new EditRequestCommand
            {
                RequestModel = new EditRequestViewModel { Id = _existingRequest.RequestId, Address = _existingRequest.Address, City = _existingRequest.City, State = _existingRequest.State, Zip = _existingRequest.Zip }
            });

            mockGeocoder.Verify(x => x.Geocode(_existingRequest.Address, _existingRequest.City, _existingRequest.State, _existingRequest.Zip, It.IsAny<string>()), Times.Once);
        }
    }
}
