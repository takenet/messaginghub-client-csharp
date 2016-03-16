## Testing

In order to ease the effort required to test your application, there is a starter pack for a Console Application host.

Add a new project for a Console Application and from the Package Manager Console, install it using:

    Install-Package Takenet.MessagingHub.Client.ConsoleHost

*Note: this package targets framework 4.6.1, so change your project target framework accordingly.*

## Production

On production environments a more robust solution, such as a Windows Service for instance, must be used. 
