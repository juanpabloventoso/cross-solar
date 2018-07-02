using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CrossSolar.Controllers;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit;

namespace CrossSolar.Tests.Controller
{
			
	internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
	{
		private readonly IQueryProvider _inner;

		internal TestAsyncQueryProvider(IQueryProvider inner)
		{
			_inner = inner;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			return new TestAsyncEnumerable<TEntity>(expression);
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new TestAsyncEnumerable<TElement>(expression);
		}

		public object Execute(Expression expression)
		{
			return _inner.Execute(expression);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return _inner.Execute<TResult>(expression);
		}

		public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
		{
			return new TestAsyncEnumerable<TResult>(expression);
		}

		public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute<TResult>(expression));
		}
	}

	internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
	{
		public TestAsyncEnumerable(IEnumerable<T> enumerable)
			: base(enumerable)
		{ }

		public TestAsyncEnumerable(Expression expression)
			: base(expression)
		{ }

		public IAsyncEnumerator<T> GetEnumerator()
		{
			return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
		}

		IQueryProvider IQueryable.Provider
		{
			get { return new TestAsyncQueryProvider<T>(this); }
		}
	}

	internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _inner;

		public TestAsyncEnumerator(IEnumerator<T> inner)
		{
			_inner = inner;
		}

		public void Dispose()
		{
			_inner.Dispose();
		}

		public T Current
		{
			get
			{
				return _inner.Current;
			}
		}

		public Task<bool> MoveNext(CancellationToken cancellationToken)
		{
			return Task.FromResult(_inner.MoveNext());
		}
	}
	
    public class AnalyticsControllerTests
    {
        private AnalyticsController _analyticsController;

        private Mock<IAnalyticsRepository> _analyticsRepositoryMock = new Mock<IAnalyticsRepository>();
        private Mock<IDayAnalyticsRepository> _dayAnalyticsRepositoryMock = new Mock<IDayAnalyticsRepository>();
        private Mock<IPanelRepository> _panelRepositoryMock = new Mock<IPanelRepository>();

        public AnalyticsControllerTests()
        {
			// Create the Panel async enumerator with test data
			var panelData = new List<Panel> 
            { 
                new Panel{ Id = 1, Serial = "AAAA1111BBBB2222", Latitude = -35.492758, Longitude = 80.947992 }, 
                new Panel{ Id = 2, Serial = "CCCC3333DDDD4444", Latitude = 22.199302, Longitude = 139.400192 }
            }.AsQueryable(); 			
			var panelMockSet = new Mock<DbSet<Panel>>(); 
			panelMockSet.As<IAsyncEnumerable<Panel>>().Setup(m => m.GetEnumerator()).Returns(new TestAsyncEnumerator<Panel>(panelData.GetEnumerator()));
			panelMockSet.As<IQueryable<Panel>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Panel>(panelData.Provider));
            panelMockSet.As<IQueryable<Panel>>().Setup(m => m.Expression).Returns(panelData.Expression); 
            panelMockSet.As<IQueryable<Panel>>().Setup(m => m.ElementType).Returns(panelData.ElementType); 
            panelMockSet.As<IQueryable<Panel>>().Setup(m => m.GetEnumerator()).Returns(panelData.GetEnumerator()); 
            _panelRepositoryMock.Setup(m => m.Query()).Returns(panelMockSet.Object.AsQueryable()); 			

			// Create the one hour analytics async enumerator with test data
			var analyticsData = new List<OneHourElectricity> 
            { 
                new OneHourElectricity{ Id = 1, PanelId = "AAAA1111BBBB2222", KiloWatt = 12, DateTime = DateTime.UtcNow }, 
                new OneHourElectricity{ Id = 2, PanelId = "CCCC3333DDDD4444", KiloWatt = 31, DateTime = DateTime.UtcNow }
            }.AsQueryable(); 			
			var analyticsMockSet = new Mock<DbSet<OneHourElectricity>>(); 
			analyticsMockSet.As<IAsyncEnumerable<OneHourElectricity>>().Setup(m => m.GetEnumerator()).Returns(new TestAsyncEnumerator<OneHourElectricity>(analyticsData.GetEnumerator()));
			analyticsMockSet.As<IQueryable<OneHourElectricity>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<OneHourElectricity>(analyticsData.Provider));
            analyticsMockSet.As<IQueryable<OneHourElectricity>>().Setup(m => m.Expression).Returns(analyticsData.Expression); 
            analyticsMockSet.As<IQueryable<OneHourElectricity>>().Setup(m => m.ElementType).Returns(analyticsData.ElementType); 
            analyticsMockSet.As<IQueryable<OneHourElectricity>>().Setup(m => m.GetEnumerator()).Returns(analyticsData.GetEnumerator()); 
            _analyticsRepositoryMock.Setup(m => m.Query()).Returns(analyticsMockSet.Object.AsQueryable()); 			
				
			// Create the day analytics async enumerator with test data
			var dayAnalticsData = new List<OneDayElectricity> 
            { 
                new OneDayElectricity{ Id = 1, PanelId = "AAAA1111BBBB2222", Sum = 50.5, Average = 25.50, Maximum = 112.35, Minimum = 11.1, DateTime = DateTime.UtcNow }, 
                new OneDayElectricity{ Id = 2, PanelId = "CCCC3333DDDD4444", Sum = 150.5, Average = 225.50, Maximum = 312.35, Minimum = 111.1, DateTime = DateTime.UtcNow }
            }.AsQueryable(); 			
			var dayAnalyticsMockSet = new Mock<DbSet<OneDayElectricity>>(); 
			dayAnalyticsMockSet.As<IAsyncEnumerable<OneDayElectricity>>().Setup(m => m.GetEnumerator()).Returns(new TestAsyncEnumerator<OneDayElectricity>(dayAnalticsData.GetEnumerator()));
			dayAnalyticsMockSet.As<IQueryable<OneDayElectricity>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<OneDayElectricity>(dayAnalticsData.Provider));
            dayAnalyticsMockSet.As<IQueryable<OneDayElectricity>>().Setup(m => m.Expression).Returns(dayAnalticsData.Expression); 
            dayAnalyticsMockSet.As<IQueryable<OneDayElectricity>>().Setup(m => m.ElementType).Returns(dayAnalticsData.ElementType); 
            dayAnalyticsMockSet.As<IQueryable<OneDayElectricity>>().Setup(m => m.GetEnumerator()).Returns(dayAnalticsData.GetEnumerator()); 
            _dayAnalyticsRepositoryMock.Setup(m => m.Query()).Returns(dayAnalyticsMockSet.Object.AsQueryable()); 		
			
			_analyticsController = new AnalyticsController(_analyticsRepositoryMock.Object, 
				_dayAnalyticsRepositoryMock.Object, _panelRepositoryMock.Object);
        }


        [Fact]
        public async Task Get_ShouldReturnOneHourElectricityListModel()
        {
            // Act
            var result = await _analyticsController.Get("AAAA1111BBBB2222");

            // Assert
            Assert.NotNull(result);
            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
        }


        [Fact]
        public async Task DayResults_ShouldReturnOneDayElectricityListModel()
        {
            // Act
            var result = await _analyticsController.DayResults("AAAA1111BBBB2222");

            // Assert
            Assert.NotNull(result);
            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
        }
		
		
        [Fact]
        public async Task Post_ShouldInsertOneHourElectricityModel()
        {
            var model = new OneHourElectricityModel
            {
                DateTime = System.DateTime.UtcNow,
                KiloWatt = 4200
            };

            // Act
            var result = await _analyticsController.Post("AAAA1111BBBB2222", model);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
        }

    }
}
