﻿using fakebook.Models;

namespace fakebook.DTO.v1.User;

public class UserUpdateDTO
{
    public string? ExistingPassword { get; set; }  // Don't want these for NEW users
    public string? NewPassword { get; set; }  // Don't want these for NEW users
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? About { get; set; }
    public string? Status { get; set; }
}
