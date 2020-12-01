/* 
This generates random data for test.
It's only designed to run after running setup.sql because it depends on postgres sequences to be in their initial state.
*/


insert into users 
(us_username, us_email, us_is_active, us_password) 
values('inactive', 'inactive@example.com', false, 'anything');

insert into users 
(us_username, us_email, us_is_report_only, us_password) 
values('report', 'report_only@example.com', false, 'anything');

insert into users 
(us_username, us_email, us_organization, us_password) 
values('org', 'belongs_to_organization@example.com', 2, 'anything');

/* create issues */
DO $$ 
DECLARE
   my_int int := 0;
   my_s varchar(30);
   number_of_issues int := 100; /* how many issues to generate */
BEGIN

/* create issues */
loop
   my_int := my_int + 1;
   my_s := cast (my_int as varchar);
  
   insert into issues (
   i_description, 
   i_created_by_user, 
   i_assigned_to_user,
   i_organization,
   i_category, i_project, i_status, i_priority)
   values (
   
   /* a random string for the description */
   my_s || ' ' ||  substr(md5(random()::text), 0, floor(random() * 30 + 1)::int),
   
   /* created by,. random users from 1 to 4 */
   floor(random() * 4 + 1)::int,

   /* assigned to, random users from 0 to 4 */
   floor(random() * 5 )::int,
   
   /* random numbers from 0 to 3 for the category, project, etc */
   floor(random() * 4)::int,
   floor(random() * 4 )::int,
   floor(random() * 4 )::int,
   floor(random() * 4 )::int,
   floor(random() * 4 )::int
   );

   if my_int = number_of_issues then
      exit; 
   end if;
end loop;

/* for the last issue created, create 10 posts  */
my_int := 0;
loop
   my_int := my_int + 1;
   insert into posts (p_issue, p_post_type, p_text, p_created_by_user)
   values(
      number_of_issues,
      'comment',
      substr(md5(random()::text), 0, floor(random() * 30 + 1)::int),
      1);
   if my_int > 10 then
      exit; 
   end if;
end loop;

END $$;
