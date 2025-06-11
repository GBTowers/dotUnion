## Release 1.0

### New Rules

| Rule ID | Category | Severity | Notes                                            |
|---------|----------|----------|--------------------------------------------------|
| UL1001  | Union    | Error    | Union target must be partial                     |
| UL1002  | Union    | Error    | Union parents must be partial                    |
| UL1003  | Union    | Error    | Union type cannot be sealed                      |
| UL1004  | Union    | Error    | Union target cannot have base type               |
| UL1005  | Union    | Error    | Union parent cannot have non-private constructor |
| UL1006  | Union    | Error    | Union type can only have one non-generated part  |
| UL2001  | Union    | Error    | Union member must be partial                     |
| UL2002  | Union    | Error    | Union member cannot be generic                   |
| UL2003  | Union    | Error    | Union member cannot have base type               |
| UL2004  | Union    | Info     | Union type member must be a record and partial   |
| UL2005  | Union    | Error    | Union type member must be public                 |
