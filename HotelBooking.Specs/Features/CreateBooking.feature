Feature: CreateBooking
	In order to book a room
	as a customer, I would like 
	to know if I can book a room. 

@mytag
Scenario: Customer tries to book a room
	Given the start date is <startDate> 
	And the end date is <endDate>
	When I press book
	Then the result should be <result>
	
	Examples:
	| startDate  | endDate    | result |
	| 2026-03-17 | 2026-03-18 | true   |