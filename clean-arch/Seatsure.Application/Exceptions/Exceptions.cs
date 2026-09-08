namespace Seatsure.Application.Exceptions;

public abstract class AppException : Exception
{
    public abstract int SatusCode { get; }
    protected AppException(string message) : base(message) { }
}

public sealed class NotFoundException : AppException { public override int SatusCode => 404; public NotFoundException(string message) : base(message) { } }
public sealed class ValidationException : AppException { public override int SatusCode => 400; public ValidationException(string message) : base(message) { } }
public sealed class ConflictException : AppException { public override int SatusCode => 409; public ConflictException(string message) : base(message) { } }
public sealed class EmailTakenException : AppException { public override int SatusCode => 409; public EmailTakenException(string message) : base(message) { } }
public sealed class UnauthorizedException : AppException { public override int SatusCode => 401; public UnauthorizedException(string message) : base(message) { } }
public sealed class ForbiddenException : AppException { public override int SatusCode => 403; public ForbiddenException(string message) : base(message) { } }
