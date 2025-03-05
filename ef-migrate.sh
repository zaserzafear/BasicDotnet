#!/bin/bash

# Set the project directories (adjust these as per your project structure)
INFRA_PROJECT="BasicDotnet.Infra"
API_PROJECT="BasicDotnet.WebApi"

# Start an infinite loop to keep the program running
while true; do
    # Display the options to the user
    echo "Select an option:"
    echo "1. Add Migration"
    echo "2. Apply Migration"
    echo "3. Generate SQL for Migration"
    echo "4. Remove Last Migration"
    echo "5. Exit"
    echo -n "Enter your choice (1, 2, 3, 4, or 5): "
    read choice

    # Based on the user's choice, perform the corresponding action
    case $choice in
        1)
            # Add a new migration
            echo "Please enter the name of the migration:"
            read migration_name
            echo "Adding migration: $migration_name"
            dotnet ef migrations add "$migration_name" --project "$INFRA_PROJECT" --startup-project "$API_PROJECT"

            # Check if migration add was successful
            if [ $? -eq 0 ]; then
                echo "Migration '$migration_name' added successfully!"
            else
                echo "Error: Migration '$migration_name' failed!"
            fi
            ;;
        2)
            # Apply migrations
            echo "Applying migrations on project: $INFRA_PROJECT"
            dotnet ef database update --project "$INFRA_PROJECT" --startup-project "$API_PROJECT"

            # Check if migration was successful
            if [ $? -eq 0 ]; then
                echo "Database migration applied successfully!"
            else
                echo "Error: Database migration failed!"
            fi
            ;;
        3)
            # Generate SQL for migrations
            echo "Generating SQL for migration..."

            # Ask for FROM (starting migration)
            echo -n "Enter the FROM migration (press Enter to use '0' as default): "
            read from_migration
            from_migration=${from_migration:-0}

            # Ask for TO (target migration)
            echo -n "Enter the TO migration (press Enter to use the last migration as default): "
            read to_migration
            to_migration=${to_migration:-"last"}

            # Ask for output file name
            echo -n "Enter the output file name (leave blank for no file): "
            read output_file

            if [ "$from_migration" = "0" ] && [ "$to_migration" = "last" ]; then
                # Generate SQL without specifying from/to migration (default behavior)
                if [ -z "$output_file" ]; then
                    dotnet ef migrations script --project "$INFRA_PROJECT" --startup-project "$API_PROJECT"
                else
                    dotnet ef migrations script --project "$INFRA_PROJECT" --startup-project "$API_PROJECT" -o "$output_file"
                fi
            else
                # Generate SQL with specified from/to migrations
                if [ -z "$output_file" ]; then
                    dotnet ef migrations script "$from_migration" "$to_migration" --project "$INFRA_PROJECT" --startup-project "$API_PROJECT"
                else
                    dotnet ef migrations script "$from_migration" "$to_migration" --project "$INFRA_PROJECT" --startup-project "$API_PROJECT" -o "$output_file"
                fi
            fi

            # Check if SQL generation was successful
            if [ $? -eq 0 ]; then
                echo "SQL for migration generated successfully!"
                [ -n "$output_file" ] && echo "Saved to '$output_file'"
            else
                echo "Error: SQL generation failed!"
            fi
            ;;
        4)
            # Remove the last migration
            echo "Removing the last migration..."
            
            # Check if there are any migrations before attempting to remove
            LAST_MIGRATION=$(dotnet ef migrations list --project "$INFRA_PROJECT" --startup-project "$API_PROJECT" | tail -n 1)

            if [ -z "$LAST_MIGRATION" ]; then
                echo "Error: No migrations found!"
            else
                dotnet ef migrations remove --project "$INFRA_PROJECT" --startup-project "$API_PROJECT"

                # Check if removal was successful
                if [ $? -eq 0 ]; then
                    echo "Last migration '$LAST_MIGRATION' removed successfully!"
                else
                    echo "Error: Failed to remove last migration!"
                fi
            fi
            ;;
        5)
            # Exit the loop and program
            echo "Exiting the program."
            break
            ;;
        *)
            # Invalid option entered
            echo "Invalid option. Please select 1, 2, 3, 4, or 5."
            ;;
    esac
done
