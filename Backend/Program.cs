using Backend.Models;
using Backend.ProgramConfig;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Force backend to listen only on port 5236 (HTTP). Frontend will call this port explicitly.
builder.WebHost.UseUrls("http://localhost:5236");

builder.Services.AddMyServices1(builder.Configuration);
builder.Services.AddMyServices2();
builder.Services.AddMyServices3();
builder.Services.AddMyServices4();
builder.Services.AddMyServices5();
builder.Services.AddMyServices6();
builder.Services.AddMyServices7();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure CORS: allow the frontend origin and allow credentials for SignalR
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
	{
		policyBuilder
			.WithOrigins("http://localhost:5055") // frontend origin - adjust if your frontend runs on a different origin
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials(); // required for SignalR negotiate when credentials mode is 'include'
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Apply the specific-origins CORS policy globally
app.UseCors("AllowSpecificOrigins");

// Note: HTTPS redirection is disabled to keep the backend listening on http://localhost:5236
// app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
// Map SignalR hub and require the same CORS policy for hub endpoints
app.MapHub<Backend.Hubs.ChatHub>("/hubs/chat").RequireCors("AllowSpecificOrigins");

// Development helper: optionally remove Message table FK constraints so messages can be saved
// using only senderId/receiverId integers (useful for demo environments where users may not exist).
// WARNING: This will modify the database schema and remove referential integrity for Message -> User.
if (app.Environment.IsDevelopment())
{
	try
	{
		using var scope = app.Services.CreateScope();
		var ctx = scope.ServiceProvider.GetRequiredService<Backend.Models.CloneEbayDbContext>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		logger.LogInformation("Development startup: checking for Message FK constraints to drop (if present)");

		// Drop foreign keys if they exist (names are from the current model). This is safe to run repeatedly.
		var dropFk1 = @"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Message__receive__5DCAEF64')
						ALTER TABLE [Message] DROP CONSTRAINT [FK__Message__receive__5DCAEF64];";
		var dropFk2 = @"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK__Message__senderI__5CD6CB2B')
						ALTER TABLE [Message] DROP CONSTRAINT [FK__Message__senderI__5CD6CB2B];";
		var dropIdx1 = @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Message_receiverId' AND object_id = OBJECT_ID('Message'))
						DROP INDEX IX_Message_receiverId ON [Message];";
		var dropIdx2 = @"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Message_senderId' AND object_id = OBJECT_ID('Message'))
						DROP INDEX IX_Message_senderId ON [Message];";

		// Execute SQL (synchronously to ensure it runs before requests)
		ctx.Database.ExecuteSqlRaw(dropFk1);
		ctx.Database.ExecuteSqlRaw(dropFk2);
		ctx.Database.ExecuteSqlRaw(dropIdx1);
		ctx.Database.ExecuteSqlRaw(dropIdx2);
		// Also drop ProductId column from Message if present (make Message standalone)
		var dropColumn = @"IF COL_LENGTH('Message','ProductId') IS NOT NULL
							BEGIN
								ALTER TABLE [Message] DROP COLUMN [ProductId];
							END";
		ctx.Database.ExecuteSqlRaw(dropColumn);
		logger.LogInformation("Development startup: attempted to drop Message FK/index constraints (if present)");
	}
	catch (Exception ex)
	{
		// do not crash startup for this convenience operation; just log
		var logger = app.Services.GetRequiredService<ILogger<Program>>();
		logger.LogWarning(ex, "Failed to drop Message FK constraints during development startup: {Message}", ex.Message);
	}
}

app.Run();
