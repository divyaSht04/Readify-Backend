using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.enums;

namespace Backend;

[Table("Users")]
public class Users
{
    [Key]
    public Guid Id { get; set; }
    
    [Column("full_name"), MaxLength(100), Required]
    public string Name { get; set; }
    
    [Column(name:"phone_number"), MaxLength(100), Required]
    public string PhoneNumber { get; set; }
    
    
    [Column("email"), MaxLength(100), Required]
    public string Email { get; set; }
    
    [Column("password"), Required, MaxLength(255)]
    public string Password { get; set; }
    
    [Column("address"), MaxLength(100), Required]
    public string? Address { get; set; }
    
    [Column("image"), MaxLength(100)]
    public string? Image { get; set; }

    public Roles Role { get; set; } = Roles.USER;
    
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;
    
    [Column("otp_code"), MaxLength(10)]
    public string? OtpCode { get; set; }
    
    [Column("otp_expiry")]
    public DateTime? OtpExpiryTime { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    
    [Column("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [Column("refresh_token_expiry")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}