/* 
This generates random data for testing and demo'ing.
Run setup.sql first
Then run this
When you are done playing and want the real thing, run setup.sql again
*/

/* create issues */
DO $$ 
DECLARE
   my_counter int := 0;
   my_s text;
   number_of_issues int := 4000; /* how many issues to generate */
   my_array text[] := '{"dog", "cat", "human", 
      "apple", "orange", "tree", "water", "rock", "sun", "moon", "bug", "violin"}';
BEGIN

/* create issues */
loop
   my_counter := my_counter + 1;
  
   insert into issues (
   i_description, 
   i_created_by_user, 
   i_assigned_to_user,
   i_organization,
   i_custom_1, i_custom_2, i_custom_4)
   values (
   
   /* a random string for the description */
   my_array[floor(random() * 12 + 1)::int] 
   || ' ' 
   || my_array[floor(random() * 12 + 1)::int]
   || ' ' 
   || my_array[floor(random() * 12 + 1)::int],
   

   /* created by,. random users from 1 to 8 */
   floor(random() * 8 + 1)::int,

   /* assigned to, random users from 0 to 8 */
   floor(random() * 9 )::int,
   
   /* random numbers from 0 to 3 for the org and custom fields */
   floor(random() * 4)::int, -- org
   floor(random() * 4 )::int, -- custom 1
   floor(random() * 4 )::int, -- custom 2
   floor(random() * 4 )::int -- custom 4
   );

   if my_counter = number_of_issues then
      exit; 
   end if;
end loop;

/* for the last issue created, create 10 posts  */
my_counter := 0;
loop
   my_counter := my_counter + 1;
   insert into posts (p_issue, p_post_type, p_text, p_created_by_user)
   values(
      number_of_issues,
      'comment',

      my_array[floor(random() * 12 + 1)::int] -- random string of "dog apple bug" etc
      || ' ' 
      || my_array[floor(random() * 12 + 1)::int]
      || ' ' 
      || my_array[floor(random() * 12 + 1)::int],

      1);
   if my_counter > 10 then
      exit; 
   end if;
end loop;

END $$;


insert into users 
(us_username, us_email_address, us_password) 
values('normal', 'normal@example.com', 'anything');

insert into users 
(us_username, us_email_address, us_is_active, us_password) 
values('inactive', 'inactive@example.com', false, 'anything');

insert into users 
(us_username, us_email_address, us_is_report_only, us_password) 
values('reportonly', 'report_only@example.com', true, 'anything');

insert into users 
(us_username, us_email_address, us_organization, us_password) 
values('org2', 'org2@example.com', 2, 'anything');

insert into users 
(us_username, us_email_address, us_organization, us_password) 
values('org3', 'org3@example.com', 3, 'anything');

insert into users 
(us_username, us_email_address, us_organization, us_is_report_only, us_password) 
values('org2_and_reportonly', 'org2_reportonly@example.com', 2, true, 'anything');

insert into users 
(us_username, us_email_address, us_is_admin, us_password) 
values('anotheradmin', 'another_admin@example.com', true, 'anything');

