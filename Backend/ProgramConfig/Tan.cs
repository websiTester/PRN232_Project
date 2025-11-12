using Backend.Repositories;
using Backend.Services;

namespace Backend.ProgramConfig
{
	public static class Tan
	{
		public static IServiceCollection AddMyServices3(this IServiceCollection services)
		{
            services.AddScoped<IReviewRepository2, ReviewRepository2>();
            services.AddScoped<IReviewService2, ReviewService2>();

            return services;
		}
	}
}
