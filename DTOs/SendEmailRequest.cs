namespace CRMapi.DTOs;

public record SendEmailRequest(

    string Subject,
    string Body,
    string To
);
